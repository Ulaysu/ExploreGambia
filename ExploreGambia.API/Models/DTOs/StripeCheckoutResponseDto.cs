namespace ExploreGambia.API.Models.DTOs
{
    public class StripeCheckoutResponseDto
    {
        public string CheckoutUrl { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;
    }
}
