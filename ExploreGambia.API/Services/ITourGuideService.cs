using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Services
{
    public interface ITourGuideService
    {
        Task<TourGuide> DeleteTourGuideAsync(Guid id);
    }
}
