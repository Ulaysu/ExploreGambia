using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateBookingRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
        public int NumberOfPeople { get; set; }

        [EnumDataType(typeof(BookingStatus), ErrorMessage = "Invalid booking status.")]
        public BookingStatus Status { get; set; }
    }
}
