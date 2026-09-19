using Domain.Interfaces;
using MailKit.Security;
using MimeKit;
using Persistence.Entities;
using Persistence.Enums;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Application.Channels;

public class EmailChannelProvider : INotificationChannelProvider
{
	private readonly INotificationChannelConfigurationService _configurationService;

	public EmailChannelProvider(INotificationChannelConfigurationService configurationService)
	{
		_configurationService = configurationService;
	}

	public ChannelType ChannelType => ChannelType.Email;

	public async Task<(bool Success, string Message)> SendNotificationAsyncA(NotificationChannel channelConfig, string recipient, string subject, string body)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(recipient))
				return (false, "El destinatario de correo electrónico está vacío.");

			var host = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_HOST");
			var port = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_PORT");
			var username = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_USERNAME");
			var password = await _configurationService.GetSecretAsync(channelConfig.Id, "SMTP_PASSWORD");
			var fromAddress = await _configurationService.GetSettingValueAsync(channelConfig.Id, "FROM_ADDRESS");
			var useSsl = await _configurationService.GetSettingValueAsync(channelConfig.Id, "USE_SSL");

			string smtpHost = host?.Value?.ToString() ?? "localhost";
			int smtpPort = port?.Value is int p ? p : 587;
			string smtpUsername = username?.Value?.ToString() ?? string.Empty;
			string smtpPassword = password ?? string.Empty;
			string from = fromAddress?.Value?.ToString() ?? "no-reply@localhost";
			bool enableSsl = useSsl?.Value is bool ssl ? ssl : true;

			if (smtpHost.Equals("localhost", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(smtpPassword) || smtpPassword == "demo123")
			{
				return (true, $"Simulación de Email a {recipient} enviada con éxito. Servidor: {smtpHost}:{smtpPort}");
			}

			using var client = new MailKit.Net.Smtp.SmtpClient();
			client.Timeout = 10000;

			var socketOptions = smtpPort switch
			{
				465 => SecureSocketOptions.SslOnConnect,
				587 => SecureSocketOptions.StartTls,
				25 => SecureSocketOptions.StartTlsWhenAvailable,
				_ => enableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None
			};

			await client.ConnectAsync(smtpHost, smtpPort, socketOptions);

			if (!string.IsNullOrWhiteSpace(smtpUsername))
			{
				client.AuthenticationMechanisms.Remove("XOAUTH2");
				await client.AuthenticateAsync(smtpUsername.Trim(), smtpPassword.Trim());
			}

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Sistema Notificaciones", from));
			foreach (var email in recipient.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				message.To.Add(MailboxAddress.Parse(email.Trim()));
			}

			message.Subject = subject;
			message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

			await client.SendAsync(message);
			await client.DisconnectAsync(true);

			return (true, $"Correo enviado correctamente a {recipient}");
		}
		catch (Exception ex)
		{
			return (false, $"Error al enviar correo electrónico: {ex.Message}");
		}
	}

	public async Task<bool> SendSmtpMail(
	SmtpClient smtpClient,
	string mailFrom,
	List<string> addressesTo,
	string subject,
	string message,
	string filename = null,
	byte[] attachment = null,
	bool isHtmlBody = false)
	{
		using MailMessage mailMessage = new()
		{
			BodyEncoding = Encoding.UTF8,
			From = new MailAddress(mailFrom),
			Priority = MailPriority.High,
			Subject = subject,
			Body = message,
			IsBodyHtml = isHtmlBody
		};

		addressesTo.ForEach(address => mailMessage.To.Add(address));

		//if (attachment != null && !string.IsNullOrWhiteSpace(filename))
		//{
		//	var contentType = new ContentType(MediaTypeNames.Application.Octet)
		//	{
		//		CharSet = Encoding.UTF8.WebName,
		//		Name = filename
		//	};

		//	var memoryStream = new MemoryStream(attachment);
		//	mailMessage.Attachments.Add(new Attachment(memoryStream, contentType));
		//}

		try
		{
			await smtpClient.SendMailAsync(mailMessage);
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return false;
	}

	public async Task<(bool Success, string Message)> SendNotificationAsync(NotificationChannel channelConfig, string recipient, string subject, string body)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(recipient))
				return (false, "El destinatario de correo electrónico está vacío.");

			var host = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_HOST");
			var port = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_PORT");
			var username = await _configurationService.GetSettingValueAsync(channelConfig.Id, "SMTP_USERNAME");
			var password = await _configurationService.GetSecretAsync(channelConfig.Id, "SMTP_PASSWORD");
			var fromAddress = await _configurationService.GetSettingValueAsync(channelConfig.Id, "FROM_ADDRESS");
			var useSsl = await _configurationService.GetSettingValueAsync(channelConfig.Id, "USE_SSL");

			string smtpHost = host?.Value?.ToString() ?? "localhost";
			int smtpPort = port?.Value is int p ? p : 587;
			string smtpUsername = username?.Value?.ToString() ?? string.Empty;
			string smtpPassword = password ?? string.Empty;
			string from = fromAddress?.Value?.ToString() ?? "no-reply@localhost";
			bool enableSsl = useSsl?.Value is bool ssl ? ssl : true;

			// Perform SMTP send if configured, or simulate send for demo/local testing
			if (smtpHost.Equals("localhost", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(smtpPassword) || smtpPassword == "demo123")
			{
				return (true, $"Simulación de Email a {recipient} enviada con éxito. Servidor: {host}:{port}");
			}

			using SmtpClient smtpClient = new()
			{
				EnableSsl = enableSsl,
				Port = smtpPort,
				Host = smtpHost,
				Credentials = new NetworkCredential(smtpUsername, smtpPassword),
				Timeout = 10000 // 10 seconds
			};


			await SendSmtpMail(smtpClient, from, [.. recipient.Split([';', ','])], subject, body);

			return (true, $"Correo enviado correctamente a {recipient}");
		}
		catch (Exception ex)
		{
			if (ex.Message.Contains("4.5.1") || ex.Message.Contains("blacklisted", StringComparison.OrdinalIgnoreCase))
			{
				return (false, "SocketLabs ha aplicado un bloqueo de protección temporal (Rate Limit / Blacklist de 5-15 min) a tu IP por los intentos fallidos de autenticación previos. Por favor aguarda unos minutos e intenta nuevamente con tu Server ID y Secret Key correctos.");
			}
			return (false, $"Error al enviar correo electrónico: {ex.Message}");
		}
	}

}
