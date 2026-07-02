using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateReviewRequestDto
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be an integer between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment text is required.")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Comment { get; set; }
    }
}
