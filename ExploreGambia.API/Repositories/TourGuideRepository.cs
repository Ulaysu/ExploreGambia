using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using ExploreGambia.API.Exceptions;

namespace ExploreGambia.API.Repositories
{
    public class TourGuideRepository : ITourGuideRepository
    {
        private const int MaxPageSize = 10;
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

        public Task<TourGuide?> GetTourGuideForDeletionAsync(Guid id)
        {
            IQueryable<TourGuide> query = context.TourGuides;

            if (context.Database.IsNpgsql())
            {
                if (context.Database.CurrentTransaction == null)
                {
                    throw new InvalidOperationException(
                        "A transaction is required when locking a tour guide for deletion.");
                }

                query = context.TourGuides.FromSqlInterpolated(
                    $"""
                    SELECT *
                    FROM "TourGuides"
                    WHERE "TourGuideId" = {id}
                    FOR UPDATE
                    """);
            }

            return query
                .Include(tourGuide => tourGuide.Verification)
                .FirstOrDefaultAsync(tourGuide => tourGuide.TourGuideId == id);
        }

        public async Task DeleteTourGuideAsync(TourGuide tourGuide)
        {
            if (tourGuide.Verification != null)
            {
                context.ProviderVerifications.Remove(tourGuide.Verification);
            }

            context.TourGuides.Remove(tourGuide);
            await context.SaveChangesAsync();
        }

        // Get a list of all TourGuides
        public async Task<List<TourGuide>> GetAllAsync(string? sortBy = null, bool isAscending = true, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            IQueryable<TourGuide> tourGuides = context.TourGuides
                .AsNoTracking()
                .Include(x => x.Tours);

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                tourGuides = tourGuides.Where(tg => EF.Functions.Like(tg.FullName, pattern));
            }

            var isSorted = false;

            // Apply sorting if sortBy parameter is provided
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "fullname":
                        tourGuides = isAscending ? tourGuides.OrderBy(tg => tg.FullName) : tourGuides.OrderByDescending(tg => tg.FullName); 
                        isSorted = true;
                        break;
                    case "availability":
                        tourGuides = isAscending ? tourGuides.OrderBy(tg => tg.IsAvailable) : tourGuides.OrderByDescending(tg => tg.IsAvailable);
                        isSorted = true;
                        break;
                    default:
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied.");
                        break;
                }
            }

            if (!isSorted)
            {
                tourGuides = tourGuides.OrderBy(tg => tg.TourGuideId);
            }

            // Apply pagination
            return await tourGuides.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        // Get a TourGuide by Id
        public async Task<TourGuide?> GetTourGuideByIdAsync(Guid id)
        {
            var tourGuide = await context.TourGuides
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TourGuideId == id);
            if (tourGuide == null) throw new TourGuideNotFoundException(id);

            return tourGuide;
        }

        public async Task<TourGuide?> GetTourGuideByUserIdAsync(string userId)
        {
            return await context.TourGuides.AsNoTracking().FirstOrDefaultAsync(g => g.UserId == userId);
        }

        public async Task<TourGuide> UpdateTourGuideProfileAsync(TourGuide guide)
        {
            context.TourGuides.Update(guide);
            await context.SaveChangesAsync();
            return guide;
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
