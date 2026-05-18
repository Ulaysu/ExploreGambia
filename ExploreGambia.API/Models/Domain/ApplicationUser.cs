using Microsoft.AspNetCore.Identity;

namespace ExploreGambia.API.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
        public DateTime?  RefreshTokenExpiryTime { get; set; }
    }
}