using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ExploreGambiaDbContext context;
        private readonly ILogger<BookingRepository> logger;

        public BookingRepository(ExploreGambiaDbContext context, ILogger<BookingRepository> logger)
        {
            this.context = context;
            this.logger = logger;
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

            if (existingBooking == null) return null;

            context.Bookings.Remove(existingBooking);
            await context.SaveChangesAsync();

            return existingBooking;

        }

        public async Task<List<Booking>> GetAllBookingsAsync(BookingStatus? status = null, 
            DateTime? bookingDateFrom = null, DateTime? bookingDateTo = null, string? sortBy = null, 
            bool isAscending = true)
        {
           IQueryable<Booking> bookings = context.Bookings.Include(b => b.Tour);

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

            // Apply sorting if sortBy parameter is provided
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "bookingdate":
                        bookings = isAscending ? bookings.OrderBy(b => b.BookingDate) : bookings.OrderByDescending(b => b.BookingDate);
                        break;
                    case "totalamount":
                        bookings = isAscending ? bookings.OrderBy(b => b.TotalAmount) : bookings.OrderByDescending(b => b.TotalAmount);
                        break;
                    case "numberofpeople":
                        bookings = isAscending ? bookings.OrderBy(b => b.NumberOfPeople) : bookings.OrderByDescending(b => b.NumberOfPeople);
                        break;
                    case "status":
                        bookings = isAscending ? bookings.OrderBy(b => b.Status) : bookings.OrderByDescending(b => b.Status);
                        break;
                    default:
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied to bookings.");
                        break;
                }
            }
            return await bookings.ToListAsync();

        }

        public async Task<Booking?> GetBookingById(Guid id)
        {
            var booking = await context.Bookings.Include(b => b.Tour).FirstOrDefaultAsync(x => x.BookingId == id);
            if (booking == null) return null;

            return booking;

        }

        // UPDATE 
        public async Task<Booking?> UpdateBookingAsync(Guid id, Booking booking)
        {
            var formattedId = id.ToString().Trim().ToLower();
            var existingBooking = await context.Bookings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.BookingId.ToString().ToLower() == formattedId);


  
            if (existingBooking == null) return null;

            var tour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == booking.TourId);

            if (tour == null) return null;

            existingBooking.TourId = booking.TourId;
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.TotalAmount = booking.NumberOfPeople * booking.Tour.Price;
            existingBooking.Status = booking.Status;


            await context.SaveChangesAsync();

            return existingBooking;

        }
    }
}
