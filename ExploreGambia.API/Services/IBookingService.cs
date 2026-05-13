using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(AddBookingRequestDto request, string userId);
        Task<Booking?> UpdateBookingAsync(Guid id, UpdateBookingRequestDto request);

        Task<List<Booking>> GetMyBookingsAsync(string userId);
    }
}
