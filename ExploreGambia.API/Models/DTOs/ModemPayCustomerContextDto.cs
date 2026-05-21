namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayCustomerContextDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}
