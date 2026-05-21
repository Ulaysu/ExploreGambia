using System.Text.Json.Serialization;

namespace ExploreGambia.API.Services.Payments
{
    public class ModemPayWebhookEvent
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public ModemPayTransaction Payload { get; set; } = new();
    }
}
