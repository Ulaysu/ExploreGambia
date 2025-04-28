using ExploreGambia.API.Data;
using ExploreGambia.API.Exceptions;
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

            if (existingTour == null) throw new TourNotFoundException(id);

            context.Tours.Remove(existingTour);
            await context.SaveChangesAsync();

            return existingTour;
        }

        // Get all Tours
        public async Task<List<Tour>> GetAllAsync(string? sortBy = null, 
            bool isAscending = true, string? location = null, decimal? minPrice = null, 
            decimal? maxPrice = null, DateTime? startDate = null, DateTime? endDate = null,
            int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<Tour> tours = context.Tours.Include("TourGuide");

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(location))
            {
                tours = tours.Where(t => t.Location.ToLower() == location.ToLower());
            }

            if (minPrice.HasValue)
            {
                tours = tours.Where(t => t.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                tours = tours.Where(t => t.Price <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                tours = tours.Where(t => t.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                tours = tours.Where(t => t.EndDate <= endDate.Value);
            }

            // Apply sorting if sortBy parameter is provided
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "title":
                        tours = isAscending ? tours.OrderBy(t => t.Title) : tours.OrderByDescending(t => t.Title);
                        break;
                    case "price":
                        tours = isAscending ? tours.OrderBy(t => t.Price) : tours.OrderByDescending(t => t.Price);
                        break;
                    case "startdate":
                        tours = isAscending ? tours.OrderBy(t => t.StartDate) : tours.OrderByDescending(t => t.StartDate);
                        break;
                    case "enddate":
                        tours = isAscending ? tours.OrderBy(t => t.EndDate) : tours.OrderByDescending(t => t.EndDate);
                        break;
                    case "location":
                        tours = isAscending ? tours.OrderBy(t => t.Location) : tours.OrderByDescending(t => t.Location);
                        break;
                    
                    default:
                        break;
                }
            }

            // Apply pagination
            return await tours.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Tour?> GetTourById(Guid id)
        {
            var tour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == id);
            if (tour == null) throw new TourNotFoundException(id);

            return tour;
        }

        // UPDATE 
        public async Task<Tour?> UpdateTourAsync(Guid id, Tour tour)
        {
            var existingTour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == id);

            if (existingTour == null) throw new TourNotFoundException(id);

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
