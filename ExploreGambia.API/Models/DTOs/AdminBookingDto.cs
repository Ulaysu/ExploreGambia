namespace ExploreGambia.API.Models.DTOs
{
    public class AdminBookingDto
    {
        public Guid BookingId { get; set; }
        public string TourTitle { get; set; }
        public string Location { get; set; }
        public string CustomerName { get; set; }

        public DateTime BookingDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
