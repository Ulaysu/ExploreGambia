using Microsoft.AspNetCore.Identity;

namespace ExploreGambia.API.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime?  RefreshTokenExpiryTime { get; set; }

        public TourGuide? TourGuide { get; set; }
    }
}
