using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ExploreGambiaDbContext _context;
        private readonly ExploreGambiaAuthDbContext _authContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminRepository(ExploreGambiaDbContext context, ExploreGambiaAuthDbContext authContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authContext = authContext;
            _userManager = userManager;
        }

        public Task<int> GetTotalUsersAsync()
        => _authContext.Users.CountAsync();

        public Task<int> GetTotalGuidesAsync()
            => _context.TourGuides.CountAsync();

        public Task<int> GetTotalToursAsync()
            => _context.Tours.CountAsync();

        public Task<int> GetTotalBookingsAsync()
            => _context.Bookings.CountAsync();

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Succeeded)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _authContext.Users.ToListAsync();

            var result = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    IsActive = user.IsActive,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Roles = roles
                });
            }

            return result;
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new AdminUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? "",
                IsActive = user.IsActive,
                Roles = roles
            };
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            user.IsActive = isActive;

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }


        public async Task<IEnumerable<Tour>> GetAllToursAsync()
        {
            return await _context.Tours
                .Include(t => t.TourGuide)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteTourAsync(Guid tourId)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.TourId == tourId);

            if (tour == null)
                return false;

            tour.IsDeleted = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreTourAsync(Guid tourId)
        {
            var tour = await _context.Tours
                .FirstOrDefaultAsync(t => t.TourId == tourId);

            if (tour == null)
                return false;

            tour.IsDeleted = false;

            await _context.SaveChangesAsync();

            return true;
        }


    }
}
