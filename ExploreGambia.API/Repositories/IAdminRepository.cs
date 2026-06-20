using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Repositories
{
    public interface IAdminRepository
    {
     
          Task<int> GetTotalUsersAsync();

          Task<int> GetTotalGuidesAsync();

          Task<int> GetTotalToursAsync();
          Task<int> GetTotalBookingsAsync();

          Task<decimal> GetTotalRevenueAsync();

        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();

        Task<AdminUserDto?> GetUserByIdAsync(string userId);

        Task<bool> UpdateUserStatusAsync(string userId, bool isActive);

        Task<IEnumerable<Tour>> GetAllToursAsync();

        Task<bool> SoftDeleteTourAsync(Guid tourId);

        Task<bool> RestoreTourAsync(Guid tourId);


    }
}
