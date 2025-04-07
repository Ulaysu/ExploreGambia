using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateBookingRequestDto
    {
        public int NumberOfPeople { get; set; } 
        public BookingStatus Status { get; set; }
    }
}
