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

        // CREATE 
        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            return booking;
        }

        // DELETE
        public async Task<Booking?> DeleteBookingAsync(Guid id)
        {
            var existingBooking = await context.Bookings.FirstOrDefaultAsync(x => x.BookingId == id);

            if (existingBooking == null) return null;

            context.Bookings.Remove(existingBooking);
            await context.SaveChangesAsync();

            return existingBooking;
        }

        // Get all Bookings
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
            var existingBooking = await context.Bookings
                .FirstOrDefaultAsync(x => x.BookingId == id);

            if (existingBooking == null) return null;

            // Fetch the updated Tour from the database
            var tour = await context.Tours.FirstOrDefaultAsync(t => t.TourId == booking.TourId);
            if (tour == null) return null; // Handle case where the new tour doesn't exist

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
