using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateBookingRequestDto
    {
        public int NumberOfPeople { get; set; } 
        public BookingStatus Status { get; set; }
    }
}
