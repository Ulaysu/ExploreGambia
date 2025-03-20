using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class TourRepository : ITourRepository
    {
        private readonly ExploreGambiaDbContext context;

        public TourRepository(ExploreGambiaDbContext context)
        {
            this.context = context;
        }

        // CREATE 
        public async Task<Tour> CreateTourAsync(Tour tour)
        {
            await context.Tours.AddAsync(tour);
            await context.SaveChangesAsync();

            return tour;
        }

        // DELETE
        public async Task<Tour?> DeleteTourAsync(Guid id)
        {
            var existingTour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == id);

            if (existingTour == null) return null;

            context.Tours.Remove(existingTour);
            await context.SaveChangesAsync();

            return existingTour;
        }

        // Get all Tours
        public async Task<List<Tour>> GetAllAsync()
        {
            return await context.Tours.ToListAsync();
        }

        public async Task<Tour?> GetTourById(Guid id)
        {
            var tour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == id);
            if (tour == null) return null;

            return tour;
        }

        // UPDATE 
        public async Task<Tour?> UpdateTourAsync(Guid id, Tour tour)
        {
            var existingTour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == id);

            if (existingTour == null) return null;

            existingTour.Title = tour.Title;
            existingTour.Description = tour.Description;
            existingTour.Location = tour.Location;
            existingTour.Price = tour.Price;
            existingTour.MaxParticipants = tour.MaxParticipants;
            existingTour.StartDate = tour.StartDate;
            existingTour.EndDate = tour.EndDate;
            existingTour.ImageUrl = tour.ImageUrl;
            existingTour.IsAvailable = tour.IsAvailable;

            await context.SaveChangesAsync();

            return existingTour;

        }
    }
}
