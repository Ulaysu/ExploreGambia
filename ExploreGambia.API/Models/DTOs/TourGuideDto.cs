namespace ExploreGambia.API.Models.DTOs
{
    public class TourGuideDto
    {
        public Guid TourGuideId { get; set; } // Primary Key

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty; // Short description of guide

        public bool IsAvailable { get; set; } = true; // Can they accept tours?
    }
}
