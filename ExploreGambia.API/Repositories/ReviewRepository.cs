using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ExploreGambiaDbContext _dbContext;

        public ReviewRepository(ExploreGambiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            // Eagerly load the user relationship after creation so the returned DTO contains the UserName.
            await _dbContext.Entry(review).Reference(r => r.User).LoadAsync();

            return review;
        }

        public async Task DeleteReviewAsync(Review review)
        {
            // Issue #5 defines review deletion as removal; add soft delete later if moderation/audit requirements emerge.
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RatingSummary> GetRatingSummaryAsync(Guid tourId)
        {
            var reviewsQuery = _dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.TourId == tourId);

            int totalReviews = await reviewsQuery.CountAsync();

            double averageRating = totalReviews > 0
                ? await reviewsQuery.AverageAsync(r => r.Rating)
                : 0.0;

            return new RatingSummary
            {
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews
            };
        }

        public async Task<Review?> GetReviewByIdAsync(Guid reviewId)
        {
            return await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByTourIdAsync(Guid tourId)
        {
            return await _dbContext.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.TourId == tourId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ReviewExistsForBookingAsync(Guid bookingId)
        {
            return await _dbContext.Reviews
                .AsNoTracking()
                .AnyAsync(r => r.BookingId == bookingId);
        }

        public async Task UpdateReviewAsync(Review review)
        {
            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(review).Reference(r => r.User).LoadAsync();
        }
    }
}
