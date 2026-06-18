namespace ExploreGambia.API.Models.DTOs
{
    public class AdminTourDto
    {
        public Guid TourId { get; set; }

        public string Title { get; set; }

        public string Location { get; set; }

        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsDeleted { get; set; }

        public string GuideName { get; set; }
    }
}
