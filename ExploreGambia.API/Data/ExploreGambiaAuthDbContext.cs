using ExploreGambia.API.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Data
{
    public class ExploreGambiaAuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public ExploreGambiaAuthDbContext(DbContextOptions<ExploreGambiaAuthDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var userRoleId = "6bcdc7ba-a89a-4a96-ba49-5838575745d1";
            var adminRoleId = "9a56e4c1-8b5e-41fb-ae0c-bf7e27662945";
            var guideRoleId = "d2c9e8f3-5b1a-4c9e-9f1a-2b3e4f5a6c7d";

            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = userRoleId,
                    ConcurrencyStamp = userRoleId,
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole
                {
                    Id = adminRoleId,
                    ConcurrencyStamp = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = guideRoleId,
                    ConcurrencyStamp = guideRoleId,
                    Name = "Guide",
                    NormalizedName = "GUIDE"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
