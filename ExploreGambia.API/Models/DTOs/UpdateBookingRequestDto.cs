using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class UpdateBookingRequestDto
    {
       public Guid TourId { get; set; } // Foreign Key (FK) to Tour

        public DateTime BookingDate { get; set; } = DateTime.UtcNow; // Timestamp
        public int NumberOfPeople { get; set; } // Number of participants

        
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
