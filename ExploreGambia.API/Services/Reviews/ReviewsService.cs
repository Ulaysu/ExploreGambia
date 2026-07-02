using AutoMapper;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;

namespace ExploreGambia.API.Services.Reviews
{
    public class ReviewsService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public ReviewsService(IReviewRepository reviewRepository, IBookingRepository bookingRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewRequestDto requestDto)
        {
            var booking = await _bookingRepository.GetBookingById(requestDto.BookingId)
                ?? throw new BookingNotFoundException(requestDto.BookingId);

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to review this booking.");

            if (booking.Status != BookingStatus.Completed)
                throw new BusinessRuleException("You can only review an experience after the booking has been completed.");

            var alreadyExists = await _reviewRepository.ReviewExistsForBookingAsync(requestDto.BookingId);
            if (alreadyExists)
                throw new BusinessRuleException("A review has already been submitted for this booking.");

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                BookingId = requestDto.BookingId,
                TourId = booking.TourId,
                UserId = userId,
                Rating = requestDto.Rating,
                Comment = requestDto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.CreateReviewAsync(review);
            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task DeleteReviewAsync(Guid reviewId, string userId, bool isAdmin)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId)
                ?? throw new ReviewNotFoundException(reviewId);

            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to delete this review.");

            await _reviewRepository.DeleteReviewAsync(review);
        }

        public async Task<RatingSummaryDto> GetRatingSummaryAsync(Guid tourId)
        {
            var summary = await _reviewRepository.GetRatingSummaryAsync(tourId);

            return new RatingSummaryDto
            {
                AverageRating = summary.AverageRating,
                TotalReviews = summary.TotalReviews
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByTourIdAsync(Guid tourId)
        {
            var reviews = await _reviewRepository.GetReviewsByTourIdAsync(tourId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto> UpdateReviewAsync(Guid reviewId, string userId, UpdateReviewRequestDto requestDto)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId)
                ?? throw new ReviewNotFoundException(reviewId);

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to update this review.");

            review.Rating = requestDto.Rating;
            review.Comment = requestDto.Comment.Trim();
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateReviewAsync(review);

            return _mapper.Map<ReviewDto>(review);
        }
    }
}
