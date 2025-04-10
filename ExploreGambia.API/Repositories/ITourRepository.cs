using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface ITourRepository
    {
        Task<List<Tour>> GetAllAsync(string? sortBy = null, bool isAscending = true);



        Task<Tour?> GetTourById(Guid id);


        Task<Tour> CreateTourAsync(Tour tour);


        Task<Tour?> UpdateTourAsync(Guid id,Tour tour);

        Task<Tour?> DeleteTourAsync(Guid id);

    }
}
