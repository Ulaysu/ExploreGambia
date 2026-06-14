namespace ExploreGambia.API.Models.DTOs
{
    public class TourParticipantDto
    {
        public Guid BookingId { get; set; }

        public string UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int NumberOfPeople { get; set; }

        public DateTime BookingDate { get; set; }
    }
}
