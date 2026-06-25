namespace ExploreGambia.API.Models.Domain
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
