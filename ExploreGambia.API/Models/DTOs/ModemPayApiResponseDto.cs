using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayApiResponseDto
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ModemPayPaymentIntentResponseDto Data { get; set; } = new();
    }
}
