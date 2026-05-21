using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class VerifyModemPayPaymentRequestDto
    {
        [Required(ErrorMessage = "TransactionId is required.")]
        [MaxLength(100, ErrorMessage = "TransactionId cannot exceed 100 characters.")]
        public string TransactionId { get; set; } = string.Empty;
    }
}
