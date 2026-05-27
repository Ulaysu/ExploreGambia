using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayPaymentInentRequestDto
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "GMD";

        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; } = string.Empty;

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = [];

        [JsonPropertyName("from_sdk")]
        public bool FromSdk { get; set; } = false;
    }
}
