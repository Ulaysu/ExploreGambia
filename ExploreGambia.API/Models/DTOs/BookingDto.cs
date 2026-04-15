using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class BookingDto
    {
        public Guid BookingId { get; set; } // Primary Key
        public TourDto Tour { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow; // Timestamp
        public int NumberOfPeople { get; set; } // Number of participants

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; } // Total cost (Price * Number of People)
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
