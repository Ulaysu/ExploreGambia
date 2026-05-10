using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.Domain
{
    public class Payment
    {
        public Guid PaymentId { get; set; }

        public Guid BookingId { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? ProviderReference { get; set; }

        public Booking Booking { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Canceled
    }
}
