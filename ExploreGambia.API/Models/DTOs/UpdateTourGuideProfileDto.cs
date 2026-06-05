using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateTourGuideProfileDto
    {
        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        [MaxLength(500)]
        public string Bio { get; set; } = string.Empty;

        [Required]
        public bool IsAvailable { get; set; }
    }
}
