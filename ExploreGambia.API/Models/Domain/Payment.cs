using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.Domain
{
    public class Payment
    {
        public Guid PaymentId { get; set; }         

        public Guid BookingId { get; set; }         

        public string PaymentMethod { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }       

        public DateTime PaymentDate { get; set; } 

        public bool IsSuccessful { get; set; }     

        public Booking Booking { get; set; }       

    }
}
