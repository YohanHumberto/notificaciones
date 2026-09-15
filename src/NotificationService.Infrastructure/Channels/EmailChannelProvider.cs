using System;
using System.Text.Json;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Channels;

public class EmailChannelProvider : INotificationChannelProvider
{
    public ChannelType ChannelType => ChannelType.Email;

    public async Task<(bool Success, string Message)> SendNotificationAsync(NotificationChannel channelConfig, string recipient, string subject, string body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recipient))
                return (false, "El destinatario de correo electrónico está vacío.");

            using var doc = JsonDocument.Parse(channelConfig.ConfigJson);
            var root = doc.RootElement;

            string host = root.TryGetProperty("Host", out var h) ? h.GetString() ?? "localhost" : "localhost";
            int port = root.TryGetProperty("Port", out var p) ? p.GetInt32() : 587;
            string username = root.TryGetProperty("Username", out var u) ? u.GetString() ?? "" : "";
            string password = root.TryGetProperty("Password", out var pwd) ? pwd.GetString() ?? "" : "";
            string fromAddress = root.TryGetProperty("FromAddress", out var fa) ? fa.GetString() ?? "no-reply@localhost" : "no-reply@localhost";
            string fromName = root.TryGetProperty("FromName", out var fn) ? fn.GetString() ?? "Sistema Notificaciones" : "Sistema Notificaciones";
            bool enableSsl = !root.TryGetProperty("EnableSsl", out var ssl) || ssl.GetBoolean();
            int timeout = root.TryGetProperty("Timeout", out var t) ? t.GetInt32() : 10000;

            // Perform SMTP send if configured, or simulate send for demo/local testing
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(password) || password == "demo123")
            {
                return (true, $"Simulación de Email a {recipient} enviada con éxito. Servidor: {host}:{port}");
            }

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Timeout = timeout;

            var socketOptions = port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                25 => SecureSocketOptions.StartTlsWhenAvailable,
                _ => enableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None
            };

            await client.ConnectAsync(host, port, socketOptions);

            if (!string.IsNullOrWhiteSpace(username))
            {
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(username.Trim(), password.Trim());
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
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
            if (ex.Message.Contains("4.5.1") || ex.Message.Contains("blacklisted", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "SocketLabs ha aplicado un bloqueo de protección temporal (Rate Limit / Blacklist de 5-15 min) a tu IP por los intentos fallidos de autenticación previos. Por favor aguarda unos minutos e intenta nuevamente con tu Server ID y Secret Key correctos.");
            }
            return (false, $"Error al enviar correo electrónico: {ex.Message}");
        }
    }
}
