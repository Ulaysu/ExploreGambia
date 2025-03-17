using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.Domain
{
    public class Booking
    {
        public Guid BookingId { get; set; } // Primary Key
        public Guid TourId { get; set; } // Foreign Key (FK) to Tour
        
        
        public DateTime BookingDate { get; set; } = DateTime.UtcNow; // Timestamp
        public int NumberOfPeople { get; set; } // Number of participants

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; } // Total cost (Price * Number of People)
        public BookingStatus Status { get; set; } = BookingStatus.Pending; 

        // Navigation properties
        public Tour Tour { get; set; } 


        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }




    // Enum for Booking Status
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Canceled,
        Completed
    }
}
