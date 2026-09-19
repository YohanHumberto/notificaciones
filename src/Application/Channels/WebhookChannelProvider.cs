using Domain.Interfaces;
using Persistence.Entities;
using Persistence.Enums;
using System.Text;
using System.Text.Json;

namespace Application.Channels;

public class WebhookChannelProvider : INotificationChannelProvider
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly INotificationChannelConfigurationService _configurationService;

	public WebhookChannelProvider(IHttpClientFactory httpClientFactory, INotificationChannelConfigurationService configurationService)
	{
		_httpClientFactory = httpClientFactory;
		_configurationService = configurationService;
	}

	public ChannelType ChannelType => ChannelType.Webhook;

	public async Task<(bool Success, string Message)> SendNotificationAsync(NotificationChannel channelConfig, string recipient, string subject, string body)
	{
		try
		{
			var urlSetting = await _configurationService.GetSettingValueAsync(channelConfig.Id, "URL");
			var methodSetting = await _configurationService.GetSettingValueAsync(channelConfig.Id, "HTTP_METHOD");
			var tokenSetting = await _configurationService.GetSecretAsync(channelConfig.Id, "AUTH_TOKEN");

			string targetUrl = !string.IsNullOrWhiteSpace(recipient)
				? recipient
				: (urlSetting?.Value?.ToString() ?? string.Empty);

			if (string.IsNullOrWhiteSpace(targetUrl))
				return (false, "No se especificó la URL de destino del Webhook.");

			string httpMethod = methodSetting?.Value?.ToString() ?? "POST";

			var client = _httpClientFactory.CreateClient();
			if (!string.IsNullOrWhiteSpace(tokenSetting))
			{
				client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", tokenSetting);
			}

			string jsonPayload = body;
			if (!IsJson(body))
			{
				var payloadObj = new
				{
					eventSubject = subject,
					message = body,
					timestamp = DateTime.UtcNow
				};
				jsonPayload = JsonSerializer.Serialize(payloadObj);
			}

			var request = new HttpRequestMessage(new HttpMethod(httpMethod), targetUrl)
			{
				Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			if (response.IsSuccessStatusCode)
			{
				return (true, $"Webhook enviado exitosamente a {targetUrl} (Status HTTP {(int)response.StatusCode})");
			}
			else
			{
				return (false, $"Webhook a {targetUrl} devolvió código de error HTTP {(int)response.StatusCode}");
			}
		}
		catch (Exception ex)
		{
			return (false, $"Excepción al procesar Webhook: {ex.Message}");
		}
	}

	private static bool IsJson(string input)
	{
		if (string.IsNullOrWhiteSpace(input)) return false;
		input = input.Trim();
		return (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]"));
	}
}
