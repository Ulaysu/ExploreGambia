using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class PaymentInentRequestDto
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
        public Dictionary<string, string> Metadata { get; set; } = new();

        [JsonPropertyName("network")]
        public string? Network { get; set; }

        [JsonPropertyName("account_number")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("skip_url_validation")]
        public bool SkipUrlValidation { get; set; } = false;
    }
}
