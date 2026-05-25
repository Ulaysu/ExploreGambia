using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class PaymentIntentResponseDto
    {
        [JsonPropertyName("payment_intent_id")]
        public string PaymentIntentId { get; set; } = string.Empty;

        [JsonPropertyName("intent_secret")]
        public string IntentSecret { get; set; } = string.Empty;

        [JsonPropertyName("payment_link")]
        public string PaymmentLink {  get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

    }
}
