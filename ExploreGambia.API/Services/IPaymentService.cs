using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(AddPaymentRequestDto request);
        Task<Payment> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequestDto request);
        Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto request);
    }
}
