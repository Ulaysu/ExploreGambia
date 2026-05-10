using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Models.DTOs
{
    public class ConfirmPaymentRequestDto
    {
        [MaxLength(100, ErrorMessage = "ProviderReference cannot exceed 100 characters.")]
        public string? ProviderReference { get; set; }
    }
}
