namespace ExploreGambia.API.Models.DTOs
{
    public class CreateModemPayIntentRequestDto
    {
        public string ReturnUrl { get; set; } = string.Empty;

        public string CancelUrl { get; set; } = string.Empty;
    }
}
