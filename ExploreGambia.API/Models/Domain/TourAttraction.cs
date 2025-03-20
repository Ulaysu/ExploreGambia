namespace ExploreGambia.API.Models.Domain
{
    public class TourAttraction
    {
        public Guid TourId { get; set; } // Foreign Key
        public Tour Tour { get; set; }

        public Guid AttractionId { get; set; } // Foreign Key
        public Attraction Attraction { get; set; }
    }
}
