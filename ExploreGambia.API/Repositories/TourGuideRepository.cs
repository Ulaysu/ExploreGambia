using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using ExploreGambia.API.Exceptions;

namespace ExploreGambia.API.Repositories
{
    public class TourGuideRepository : ITourGuideRepository
    {
        private readonly ExploreGambiaDbContext context;

        public TourGuideRepository(ExploreGambiaDbContext  context)
        {
            this.context = context;
        }
        public async Task<TourGuide> CreateTourGuideAsync(TourGuide tourGuide)
        {
            await context.TourGuides.AddAsync(tourGuide);
            await context.SaveChangesAsync();

            return tourGuide;
        }

        // Delete a TourGuide by Id
        public async Task<TourGuide?> DeleteTourGuideAsync(Guid id)
        {
            var existingTourGuide = await context.TourGuides.FirstOrDefaultAsync(x => x.TourGuideId == id);

            if (existingTourGuide == null) throw new TourGuideNotFoundException(id);

            context.TourGuides.Remove(existingTourGuide);
            await context.SaveChangesAsync();

            return existingTourGuide;
        }

        // Get a list of all TourGuides
        public async Task<List<TourGuide>> GetAllAsync()
        {
            return await context.TourGuides.ToListAsync();
        }

        // Get a TourGuide by Id
        public async Task<TourGuide?> GetTourGuideByIdAsync(Guid id)
        {
            var tourGuide = await context.TourGuides.FirstOrDefaultAsync(x => x.TourGuideId == id);
            if (tourGuide == null) throw new TourGuideNotFoundException(id);

            return tourGuide;
        }

        // Update a TourGuide
        public async Task<TourGuide?> UpdateTourGuideAsync(Guid id, TourGuide tourGuide)
        {
            var existingTourGuide = await context.TourGuides.FirstOrDefaultAsync(x => x.TourGuideId == id);

            if (existingTourGuide == null) throw new TourGuideNotFoundException(id);

            existingTourGuide.FullName = tourGuide.FullName;
            existingTourGuide.PhoneNumber = tourGuide.PhoneNumber;
            existingTourGuide.Email = tourGuide.Email;
            existingTourGuide.Bio = tourGuide.Bio;
            existingTourGuide.IsAvailable = tourGuide.IsAvailable;

            await context.SaveChangesAsync();

            return existingTourGuide;
        }
    }
}
