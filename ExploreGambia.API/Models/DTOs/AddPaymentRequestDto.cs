using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddPaymentRequestDto
    {
        public Guid BookingId { get; set; }

        public string PaymentMethod { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public bool IsSuccessful { get; set; }

    }
}
