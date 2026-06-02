using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateAuthMeRequestDto
    {
        [Required]
        [RegularExpression(@".*\S.*", ErrorMessage = "First name cannot be blank.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@".*\S.*", ErrorMessage = "Last name cannot be blank.")]
        public string LastName { get; set; } = string.Empty;
    }
}
