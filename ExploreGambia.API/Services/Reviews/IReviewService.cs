using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewRequestDto requestDto);
        Task<IEnumerable<ReviewDto>> GetReviewsByTourIdAsync(Guid tourId);
        Task<RatingSummaryDto> GetRatingSummaryAsync(Guid tourId);
        Task<ReviewDto> UpdateReviewAsync(Guid reviewId, string userId, UpdateReviewRequestDto requestDto);
        Task DeleteReviewAsync(Guid reviewId, string userId, bool isAdmin);
    }
}
