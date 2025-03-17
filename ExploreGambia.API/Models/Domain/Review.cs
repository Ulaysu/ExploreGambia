namespace ExploreGambia.API.Models.Domain
{
    public class Review
    {
        public Guid ReviewId { get; set; }          // Unique identifier for the review
        public Guid TourId { get; set; }            // The tour the review is associated with
        public string UserId { get; set; }          // The user who left the review (Identity User ID)
        public string Comment { get; set; }        // Review text
        public int Rating { get; set; }            // Rating (e.g., 1-5 stars)
        public DateTime ReviewDate { get; set; }   // Date when the review was left

        // Navigation Properties
        public Tour Tour { get; set; }             // Navigation to the associated tour
        //public ApplicationUser User { get; set; }  // Navigation to the user who left the review to be added later


    }
}
