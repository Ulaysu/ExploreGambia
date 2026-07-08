using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateReviewAsync(Review review);
        Task<Review?> GetReviewByIdAsync(Guid reviewId);
        Task<IEnumerable<Review>> GetReviewsByTourIdAsync(Guid tourId);
        Task<RatingSummary> GetRatingSummaryAsync(Guid tourId);
        Task UpdateReviewAsync(Review review);
        Task DeleteReviewAsync(Review review);
        Task<bool> ReviewExistsForBookingAsync(Guid bookingId);
    }
}
