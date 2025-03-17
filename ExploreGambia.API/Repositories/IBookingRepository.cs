using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllBookingsAsync();


        Task<Booking?> GetBookingById(Guid id);


        Task<Booking> CreateBookingAsync(Booking booking);


        Task<Booking?> UpdateBookingAsync(Guid id, Booking booking);

        Task<Booking?> DeleteBookingAsync(Guid id);
    }
}
