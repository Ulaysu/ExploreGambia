namespace ExploreGambia.API.Models.DTOs
{
    public class ReviewDto
    {
        public Guid ReviewId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TourId { get; set; } // Kept so the API controller can generate CreatedAtAction routes correctly
    }
}
