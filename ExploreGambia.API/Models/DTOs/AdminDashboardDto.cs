namespace ExploreGambia.API.Models.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalGuides { get; set; }
        public int TotalTours { get; set; }
        public int TotalBookings { get; set; }
        public decimal Revenue { get; set; }
    }
}