using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services.Payments
{
    public interface IStripePaymentService
    {
        Task<StripeCheckoutResponseDto>
            CreateCheckoutSessionAsync(
                Guid bookingId,
                string userId,
                CreateStripeCheckoutRequestDto request);
    }
}
