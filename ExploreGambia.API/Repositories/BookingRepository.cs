using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using ExploreGambia.API.Exceptions;

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

            if (existingBooking == null) throw new BookingNotFoundException(id);

            context.Bookings.Remove(existingBooking);
            await context.SaveChangesAsync();

            return existingBooking;

        }

        public async Task<List<Booking>> GetAllBookingsAsync(BookingStatus? status = null, 
            DateTime? bookingDateFrom = null, DateTime? bookingDateTo = null, string? sortBy = null, 
            bool isAscending = true, int pageNumber = 1, int pageSize = 10)
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
            // Apply pagination
            return await bookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        }

        public async Task<Booking?> GetBookingById(Guid id)
        {
            var booking = await context.Bookings.Include(b => b.Tour).FirstOrDefaultAsync(x => x.BookingId == id);
            if (booking == null) throw new BookingNotFoundException(id);

            return booking;

        }

       

        

        // UPDATE
        public async Task<Booking?> UpdateBookingAsync(Guid id, Booking booking)
        {
            var existingBooking = await GetBookingById(id);
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
    }
}
