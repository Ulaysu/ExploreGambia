using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }

        public BookingDto Booking { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentStatus Status { get; set; }

        public string? ProviderReference { get; set; }
    }
}
