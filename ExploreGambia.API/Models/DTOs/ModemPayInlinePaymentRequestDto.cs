using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class ModemPayInlinePaymentRequestDto
    {
        [MaxLength(100, ErrorMessage = "CustomerName cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        [EmailAddress]
        [MaxLength(100, ErrorMessage = "CustomerEmail cannot exceed 100 characters.")]
        public string? CustomerEmail { get; set; }

        [MaxLength(30, ErrorMessage = "CustomerPhone cannot exceed 30 characters.")]
        public string? CustomerPhone { get; set; }
    }
}
