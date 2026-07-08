using Asp.Versioning;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpPost("reviews")]
        [Authorize(Roles ="User, Admin")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequestDto requestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var review = await reviewService.CreateReviewAsync(userId, requestDto);

            return CreatedAtAction(nameof(GetReviewsByTourId), new { tourId = review.TourId }, review);
        }

        [HttpGet("tours/{tourId:guid}/reviews")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviewsByTourId([FromRoute] Guid tourId)
        {
            var reviews = await reviewService.GetReviewsByTourIdAsync(tourId);

            return Ok(reviews);
        }

        [HttpGet("tours/{tourId:guid}/ratings")]
        [ProducesResponseType(typeof(RatingSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRatingSummary([FromRoute] Guid tourId)
        {
            var ratingSummary = await reviewService.GetRatingSummaryAsync(tourId);

            return Ok(ratingSummary);
        }

        [HttpPut("reviews/{reviewId:guid}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview([FromRoute] Guid reviewId,
            [FromBody] UpdateReviewRequestDto requestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var review = await reviewService.UpdateReviewAsync(reviewId, userId, requestDto);

            return Ok(review);
        }

        [HttpDelete("reviews/{reviewId:guid}")]
        [Authorize(Roles = "User, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview([FromRoute] Guid reviewId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await reviewService.DeleteReviewAsync(reviewId, userId, User.IsInRole("Admin"));

            return NoContent();
        }
    }
}
