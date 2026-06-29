namespace ExploreGambia.API.Models.Domain
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        // Foreign Key & Navigation to Booking (Strict 1-to-1 or 1-to-Many rule)
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        // Foreign Key & Navigation to Tour (Experience)
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }

        // Reference back to the User who wrote it (Stored as string to match Identity AspNetUsers Id)
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int Rating { get; set; } // Enforced 1-5 via DTO/Business validation
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


    }
}
