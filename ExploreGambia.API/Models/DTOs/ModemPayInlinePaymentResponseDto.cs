using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayInlinePaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PaymentMethods { get; set; } = "card";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
