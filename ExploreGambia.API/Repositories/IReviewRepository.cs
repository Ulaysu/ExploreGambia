using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateReviewAsync(Review review);
        Task<Review?> GetReviewByIdAsync(Guid reviewId);
        Task<IEnumerable<Review>> GetReviewsByTourIdAsync(Guid tourId);
        Task<RatingSummaryDto> GetRatingSummaryAsync(Guid tourId);
        Task UpdateReviewAsync(Review review);
        Task DeleteReviewAsync(Review review);
        Task<bool> ReviewExistsForBookingAsync(Guid bookingId);
    }
}
