using ExploreGambia.API.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateTourRequestDto
    {
        [GuidNotEmpty(ErrorMessage = "TourGuideId cannot be an empty GUID.")]
        public Guid TourGuideId { get; set; }

        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Precision(18, 2)] // EF Core attribute, validation handled by [Range]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max Participants must be at least 1.")]
        public int MaxParticipants { get; set; }

        public DateTime StartDate { get; set; }

        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be greater than Start Date.")]
        public DateTime EndDate { get; set; }

        [MaxLength(2048, ErrorMessage = "Image URL cannot exceed 2048 characters.")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
    }
}
