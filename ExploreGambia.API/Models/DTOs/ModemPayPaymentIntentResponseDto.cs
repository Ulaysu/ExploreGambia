using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayPaymentIntentResponseDto
    {
        [JsonPropertyName("payment_intent_id")]
        public string PaymentIntentId { get; set; } = string.Empty;

        [JsonPropertyName("intent_secret")]
        public string IntentSecret { get; set; } = string.Empty;

        [JsonPropertyName("payment_link")]
        public string PaymentLink {  get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; }

    }
}
