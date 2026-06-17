namespace ExploreGambia.API.Repositories
{
    public interface IAdminRepository
    {
     
            Task<int> GetTotalUsersAsync();
            Task<int> GetTotalGuidesAsync();
            Task<int> GetTotalToursAsync();
            Task<int> GetTotalBookingsAsync();
            Task<decimal> GetTotalRevenueAsync();
        
    }
}
