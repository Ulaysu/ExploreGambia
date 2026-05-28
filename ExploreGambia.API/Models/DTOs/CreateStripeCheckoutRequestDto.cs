namespace ExploreGambia.API.Models.DTOs
{
    public class CreateStripeCheckoutRequestDto
    {
        public string? SuccessUrl { get; set; }

        public string? CancelUrl { get; set; }
    }
}
