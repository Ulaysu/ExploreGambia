using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ExploreGambiaDbContext _context;
        private readonly ExploreGambiaAuthDbContext _authContext;

        public AdminRepository(ExploreGambiaDbContext context, ExploreGambiaAuthDbContext authContext)
        {
            _context = context;
            _authContext = authContext;
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
    }
}
