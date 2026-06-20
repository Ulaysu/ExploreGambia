using ExploreGambia.API.Data;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private const int MaxPageSize = 10;
        private readonly ExploreGambiaDbContext context;
        private readonly ExploreGambiaAuthDbContext authContext;
        private readonly ILogger<BookingRepository> logger;

        public BookingRepository(ExploreGambiaDbContext context, ILogger<BookingRepository> logger, 
            ExploreGambiaAuthDbContext authCcontext)
        {
            this.context = context;
            this.logger = logger;
            this.authContext = authCcontext;
        }
        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            return booking;

        }

        public async Task<Booking?> DeleteBookingAsync(Guid id)
        {
            var existingBooking = await context.Bookings.FirstOrDefaultAsync(x => x.BookingId == id);

            if (existingBooking == null) throw new BookingNotFoundException(id);

            context.Bookings.Remove(existingBooking);
            await context.SaveChangesAsync();

            return existingBooking;

        }

        public async Task<Booking?> GetActiveBookingByUserAndTourAsync(string userId, Guid tourId)
        {
            return await context.Bookings.Where(
                b => b.UserId == userId
                && b.TourId == tourId 
                && b.Status != BookingStatus.Completed
                && b.Status != BookingStatus.Canceled

                ).FirstOrDefaultAsync();
        }

        public async Task<List<AdminBookingDto>> GetAllBookingsAsync(
    BookingStatus? status = null,
    DateTime? bookingDateFrom = null,
    DateTime? bookingDateTo = null,
    string? sortBy = null,
    bool isAscending = true,
    int pageNumber = 1,
    int pageSize = 10)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            IQueryable<Booking> query = context.Bookings.Include(b => b.Tour).Include(b => b.User)
                .AsNoTracking();

            // Filters
            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            if (bookingDateFrom.HasValue)
                query = query.Where(b => b.BookingDate >= bookingDateFrom);

            if (bookingDateTo.HasValue)
                query = query.Where(b => b.BookingDate <= bookingDateTo.Value.AddDays(1).AddTicks(-1));

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "bookingdate" => isAscending ? query.OrderBy(b => b.BookingDate) : query.OrderByDescending(b => b.BookingDate),
                "totalamount" => isAscending ? query.OrderBy(b => b.TotalAmount) : query.OrderByDescending(b => b.TotalAmount),
                "numberofpeople" => isAscending ? query.OrderBy(b => b.NumberOfPeople) : query.OrderByDescending(b => b.NumberOfPeople),
                "status" => isAscending ? query.OrderBy(b => b.Status) : query.OrderByDescending(b => b.Status),
                _ => query.OrderBy(b => b.BookingId)
            };

            var bookings = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

               

            return bookings.Select(b => new AdminBookingDto
            {
                BookingId = b.BookingId,
                TourTitle = b.Tour.Title,
                CustomerName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}": "Guest Customer",
                TotalAmount = b.TotalAmount,
                NumberOfPeople = b.NumberOfPeople,
                Location = b.Tour.Location,
                Status = b.Status.ToString(),
                BookingDate = b.BookingDate
            }).ToList();
        }

        /*public async Task<List<Booking>> GetAllBookingsAsync(BookingStatus? status = null, 
            DateTime? bookingDateFrom = null, DateTime? bookingDateTo = null, string? sortBy = null, 
            bool isAscending = true, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

           IQueryable<Booking> bookings = context.Bookings
                .AsNoTracking()
                .Include(b => b.Tour)
                .ThenInclude(t => t.TourGuide);

            // Apply filtering
            if (status.HasValue)
            {
                bookings = bookings.Where(b => b.Status == status.Value);
            }

            if (bookingDateFrom.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= bookingDateFrom.Value);
            }

            if (bookingDateTo.HasValue)
            {
                // Consider if you want inclusive or exclusive end date
                bookings = bookings.Where(b => b.BookingDate <= bookingDateTo.Value.AddDays(1).AddTicks(-1)); // Inclusive
            }

            var isSorted = false;

            // Apply sorting if sortBy parameter is provided
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "bookingdate":
                        bookings = isAscending ? bookings.OrderBy(b => b.BookingDate) : bookings.OrderByDescending(b => b.BookingDate);
                        isSorted = true;
                        break;
                    case "totalamount":
                        bookings = isAscending ? bookings.OrderBy(b => b.TotalAmount) : bookings.OrderByDescending(b => b.TotalAmount);
                        isSorted = true;
                        break;
                    case "numberofpeople":
                        bookings = isAscending ? bookings.OrderBy(b => b.NumberOfPeople) : bookings.OrderByDescending(b => b.NumberOfPeople);
                        isSorted = true;
                        break;
                    case "status":
                        bookings = isAscending ? bookings.OrderBy(b => b.Status) : bookings.OrderByDescending(b => b.Status);
                        isSorted = true;
                        break;
                    default:
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied to bookings.");
                        break;
                }
            }

            if (!isSorted)
            {
                bookings = bookings.OrderBy(b => b.BookingId);
            }
            // Apply pagination
            return await bookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        }*/

        public async Task<Booking?> GetBookingById(Guid id)
        {
            var booking = await context.Bookings
                .AsNoTracking()
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(x => x.BookingId == id);
            if (booking == null) throw new BookingNotFoundException(id);

            return booking;

        }

        public async Task<List<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await context.Bookings.AsNoTracking().Include(b => b.Tour).OrderByDescending(b => b.BookingDate).Where(b => b.UserId == userId).ToListAsync();
        }





        // UPDATE
        public async Task<Booking?> UpdateBookingAsync(Guid id, Booking booking)
        {
            var existingBooking = await context.Bookings.FirstOrDefaultAsync(x => x.BookingId == id);
            if (existingBooking == null) throw new BookingNotFoundException(id);

            // Update basic properties
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.Status = booking.Status;

            // Update TourId and TotalAmount if a valid TourID is provided
            if (booking.TourId != Guid.Empty)
            {
                var tour = await context.Tours.FindAsync(booking.TourId);
                if (tour == null)
                {
                    throw new TourNotFoundException(booking.TourId);  
                }
                existingBooking.TourId = booking.TourId;
                existingBooking.TotalAmount = booking.NumberOfPeople * tour.Price;
            }
           

            await context.SaveChangesAsync();
            return existingBooking;
        }

        public async Task<Booking?> UpdateBookingStatusAsync(Guid id, BookingStatus status)
        {
            var existingBooking = await context.Bookings.FirstOrDefaultAsync(x => x.BookingId == id);
            if (existingBooking == null) throw new BookingNotFoundException(id);

            existingBooking.Status = status;
            existingBooking.StatusUpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return existingBooking;
        }
    }
}
