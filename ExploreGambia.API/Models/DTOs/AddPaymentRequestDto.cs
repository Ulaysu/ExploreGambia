using ExploreGambia.API.Validations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddPaymentRequestDto
    {
        [Required(ErrorMessage = "BookingId is required.")]
        [GuidNotEmpty(ErrorMessage = "BookingId cannot be an empty GUID.")]
        public Guid BookingId { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        [MaxLength(50, ErrorMessage = "PaymentMethod cannot exceed 50 characters.")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [MaxLength(100, ErrorMessage = "ProviderReference cannot exceed 100 characters.")]
        public string? ProviderReference { get; set; }
    }
}
