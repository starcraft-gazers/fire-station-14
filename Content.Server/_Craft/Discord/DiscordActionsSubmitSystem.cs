using System.Net.Http;
using System.Text.Json.Serialization;

namespace Content.Server._Craft.Discord;

public sealed class DiscrodActionsSubmitSystem : EntitySystem
{
    private readonly HttpClient _httpClient = new();
    private string _webhookUrl = String.Empty;

    private async void SendDiscordMessage(WebhookPayload payLoad)
    {

    }

    private struct WebhookPayload
    {
        [JsonPropertyName("content")] public string Content { get; set; } = "";

        public WebhookPayload()
        {
        }
    }
}
