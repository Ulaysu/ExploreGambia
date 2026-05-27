using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayCreatePaymentWrapperDto
    {
        [JsonPropertyName("data")]
        public ModemPayPaymentInentRequestDto Data { get; set; } = new();
    }
}
