namespace ExploreGambia.API.Models.Domain
{
    public class Attraction
    {
    public Guid AttractionId { get; set; } // Primary Key
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty; // Optional

    // Navigation Property for Many-to-Many Relationship
    public List<TourAttraction> TourAttractions { get; set; } = new List<TourAttraction>();
    

    }
}
