namespace ExploreGambia.API.Models.DTOs
{
    public class TourGuideProfileDto
    {
        public Guid TourGuideId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
    }
}
