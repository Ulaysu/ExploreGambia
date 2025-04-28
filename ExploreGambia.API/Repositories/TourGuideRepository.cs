using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using ExploreGambia.API.Exceptions;

namespace ExploreGambia.API.Repositories
{
    public class TourGuideRepository : ITourGuideRepository
    {
        private readonly ExploreGambiaDbContext context;
        private readonly ILogger<TourGuideRepository> logger;

        public TourGuideRepository(ExploreGambiaDbContext  context, ILogger<TourGuideRepository> logger)
        {
            this.context = context;
            this.logger = logger;
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
        public async Task<List<TourGuide>> GetAllAsync(string? sortBy = null, bool isAscending = true, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<TourGuide> tourGuides = context.TourGuides.Include(x => x.Tours);



            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                tourGuides = tourGuides.Where(tg => EF.Functions.Like(tg.FullName, pattern));
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
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied.");
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
