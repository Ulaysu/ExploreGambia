using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExploreGambia.API.Services.Payments
{
    public class ModemPayTransaction
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("payment_intent_id")]
        public string? PaymentIntentId { get; set; }

        [JsonPropertyName("transaction_reference")]
        public string? TransactionReference { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, JsonElement>? Metadata { get; set; }
    }
}
