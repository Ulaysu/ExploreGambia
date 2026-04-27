using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Validations;

namespace ExploreGambia.API.Models.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;        // ✅ Only one email field

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [ValidRoles(new string[] { "Admin", "User", "Guide" }, ErrorMessage = "Invalid role(s) selected.")]
        public string[] Roles { get; set; }
    }
}
