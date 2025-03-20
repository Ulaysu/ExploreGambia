using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.Domain
{
    public class Tour
    {
        public Guid TourId { get; set; } // Primary Key
        public string Title { get; set; } = string.Empty; // Tour title
        public string Description { get; set; } = string.Empty; // Tour details
        public string Location { get; set; } = string.Empty; // Tour location (city/region)

        [Precision(18, 2)]
        public decimal Price { get; set; }  // Tour price per person
        public int MaxParticipants { get; set; } // Max people allowed
        public DateTime StartDate { get; set; } // Tour start date
        public DateTime EndDate { get; set; } // Tour end date
        public string ImageUrl { get; set; } = string.Empty; // Optional tour image
        public bool IsAvailable { get; set; } = true; // Availability status

        // Foreign Key Relationship
        public Guid TourGuideId { get; set; } // FK reference to TourGuide

        public TourGuide TourGuide { get; set; } // Navigation Property

        // Bookings relationship
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        // Many-to-Many Relationship
        public List<TourAttraction> TourAttractions { get; set; } = new List<TourAttraction>();

        // Bookings relationship
        public List<Review> Reviews { get; set; } = new List<Review>();



    }
}
