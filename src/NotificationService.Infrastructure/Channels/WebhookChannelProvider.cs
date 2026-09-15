using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Channels;

public class WebhookChannelProvider : INotificationChannelProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookChannelProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ChannelType ChannelType => ChannelType.Webhook;

    public async Task<(bool Success, string Message)> SendNotificationAsync(NotificationChannel channelConfig, string recipient, string subject, string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(channelConfig.ConfigJson);
            var root = doc.RootElement;

            string targetUrl = !string.IsNullOrWhiteSpace(recipient)
                ? recipient
                : (root.TryGetProperty("Url", out var u) ? u.GetString() ?? "" : "");

            if (string.IsNullOrWhiteSpace(targetUrl))
                return (false, "No se especificó la URL de destino del Webhook.");

            string httpMethod = root.TryGetProperty("HttpMethod", out var m) ? m.GetString() ?? "POST" : "POST";

            var client = _httpClientFactory.CreateClient();

            if (root.TryGetProperty("Headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in headers.EnumerateObject())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(prop.Name, prop.Value.GetString());
                }
            }

            // Create payload JSON
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
