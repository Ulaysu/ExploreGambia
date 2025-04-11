using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Validations;

namespace ExploreGambia.API.Models.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [ValidRoles(new string[] { "Admin", "User" }, ErrorMessage = "Invalid role(s) selected.")]
        public string[] Roles { get; set; }

    }
}
