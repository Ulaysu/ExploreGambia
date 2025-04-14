using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            if (existingTourGuide == null) return null;

            context.TourGuides.Remove(existingTourGuide);
            await context.SaveChangesAsync();

            return existingTourGuide;
        }

        // Get a list of all TourGuides
        public async Task<List<TourGuide>> GetAllAsync(string? sortBy = null, bool isAscending = true, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<TourGuide> tourGuides = context.TourGuides.Include(x => x.Tours);



            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                tourGuides = tourGuides.Where(tg => tg.FullName.ToLower().Contains(searchTerm.ToLower()));
            }

            // Apply sorting if sortBy parameter is provided
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "fullname":
                        tourGuides = isAscending ? tourGuides.OrderBy(tg => tg.FullName) : tourGuides.OrderByDescending(tg => tg.FullName); 
                        break;
                    case "availability":
                        tourGuides = isAscending ? tourGuides.OrderBy(tg => tg.IsAvailable) : tourGuides.OrderByDescending(tg => tg.IsAvailable);
                        break;
                    default:
                        break;
                }
            }

            // Apply pagination
            return await tourGuides.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        // Get a TourGuide by Id
        public async Task<TourGuide?> GetTourGuideByIdAsync(Guid id)
        {
            var tourGuide = await context.TourGuides.FirstOrDefaultAsync(x => x.TourGuideId == id);
            if (tourGuide == null) return null;

            return tourGuide;
        }

        // Update a TourGuide
        public async Task<TourGuide?> UpdateTourGuideAsync(Guid id, TourGuide tourGuide)
        {
            var existingTourGuide = await context.TourGuides.FirstOrDefaultAsync(x => x.TourGuideId == id);

            if (existingTourGuide == null) return null;

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
