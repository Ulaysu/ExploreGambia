using System.Text.Json.Serialization;

namespace ExploreGambia.API.Models.DTOs
{
    public class CreateModemPayIntentRequestDto
    {
        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; } = string.Empty;

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; } = string.Empty;
    }
}
