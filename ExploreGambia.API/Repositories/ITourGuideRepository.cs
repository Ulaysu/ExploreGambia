using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface ITourGuideRepository
    {
        Task<List<TourGuide>> GetAllAsync();


        Task<TourGuide?> GetTourGuideByIdAsync(Guid id);


        Task<TourGuide> CreateTourGuideAsync(TourGuide tourGuide);


        Task<TourGuide?> UpdateTourGuideAsync(Guid id, TourGuide tourGuide);

        Task<TourGuide?> DeleteTourGuideAsync(Guid id);

    }
}
