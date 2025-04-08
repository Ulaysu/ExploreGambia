using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ExploreGambiaDbContext context;

        public BookingRepository(ExploreGambiaDbContext context)
        {
            this.context = context;
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

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
           return await context.Bookings.ToListAsync();

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
            var existingBooking = await GetBookingById(id);

  
  
            if (existingBooking == null) return null;

            var tour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == booking.TourId);

            if (tour == null) return null;

            existingBooking.TourId = booking.TourId;
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.TotalAmount = booking.NumberOfPeople * tour.Price;
            existingBooking.Status = booking.Status;


            await context.SaveChangesAsync();

            return existingBooking;

        }*/

        // UPDATE
        public async Task<Booking?> UpdateBookingAsync(Guid id, Booking booking)
        {
            var existingBooking = await GetBookingById(id);

  
            if (existingBooking == null) return null;

            var tour = await context.Tours.FirstOrDefaultAsync(x => x.TourId == booking.TourId);

            if (tour == null) return null;

            existingBooking.TourId = booking.TourId;
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.TotalAmount = booking.NumberOfPeople * tour.Price;
            existingBooking.Status = booking.Status;


            await context.SaveChangesAsync();

            return existingBooking;

        }*/

        // UPDATE
        public async Task<Booking?> UpdateBookingAsync(Guid id, Booking booking)
        {
            var existingBooking = await GetBookingById(id);

            if (existingBooking == null) return null;

            // Update basic properties
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.TotalAmount = booking.NumberOfPeople * booking.Tour.Price;
            existingBooking.Status = booking.Status;

            // Conditionally update TourId and TotalAmount if a valid, different TourId is provided
            if (booking.TourId != Guid.Empty && booking.TourId != existingBooking.TourId)
            {
                var tour = await context.Tours.FindAsync(booking.TourId);
                if (tour == null)
                {
                    return null;  
                }
                existingBooking.TourId = booking.TourId;
                existingBooking.TotalAmount = booking.NumberOfPeople * tour.Price;
            }
            else
            {
                // Recalculate TotalAmount based on the existing Tour if TourId wasn't changed or not provided
                var tour = await context.Tours.FindAsync(existingBooking.TourId);
                if (tour != null)
                {
                    existingBooking.TotalAmount = booking.NumberOfPeople * tour.Price;
                }
               
               
            }

            await context.SaveChangesAsync();
            return existingBooking;

        }
    }
}
