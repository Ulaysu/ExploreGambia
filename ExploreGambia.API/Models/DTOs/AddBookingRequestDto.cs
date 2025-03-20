using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddBookingRequestDto
    {
        public Guid TourId { get; set; } 

        public int NumberOfPeople { get; set; } 

        
    }
}
