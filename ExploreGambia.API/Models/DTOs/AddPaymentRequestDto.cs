using ExploreGambia.API.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddPaymentRequestDto
    {
        [Required(ErrorMessage = "BookingId is required.")]
        [GuidNotEmpty(ErrorMessage = "BookingId cannot be an empty GUID.")]
        public Guid BookingId { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        [MaxLength(50, ErrorMessage = "PaymentMethod cannot exceed 50 characters.")] 
        public string PaymentMethod { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Precision(18, 2)] 
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "PaymentDate is required.")]
        public DateTime PaymentDate { get; set; }

        public bool IsSuccessful { get; set; }
    }
}
