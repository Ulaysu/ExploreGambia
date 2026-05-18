using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllBookingsAsync(BookingStatus? status = null,
            DateTime? bookingDateFrom = null, DateTime? bookingDateTo = null, string? sortBy = null, 
            bool isAscending = true, int pageNumber = 1, int pageSize = 10);


        Task<Booking?> GetBookingById(Guid id);

        Task<Booking?> GetActiveBookingByUserAndTourAsync(string userId, Guid tourId);


        Task<Booking> CreateBookingAsync(Booking booking);

        Task<List<Booking>> GetBookingsByUserIdAsync(string userId);


        Task<Booking?> UpdateBookingAsync(Guid id, Booking booking);

        Task<Booking?> UpdateBookingStatusAsync(Guid id, BookingStatus status);

        Task<Booking?> DeleteBookingAsync(Guid id);
    }
}
