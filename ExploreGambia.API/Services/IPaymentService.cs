using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services.Payments;

namespace ExploreGambia.API.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(AddPaymentRequestDto request);
        Task<Payment> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequestDto request);
        Task<Payment> ConfirmProviderPaymentAsync(Guid id, string? providerReference);
        Task<ModemPayInlinePaymentResponseDto> PrepareModemPayInlinePaymentAsync(
            Guid bookingId,
            ModemPayInlinePaymentRequestDto request,
            ModemPayCustomerContextDto customerContext);
        Task<Payment> VerifyModemPayPaymentAsync(
            VerifyModemPayPaymentRequestDto request,
            ModemPayCustomerContextDto customerContext,
            CancellationToken cancellationToken = default);

        Task<ModemPayPaymentIntentResponseDto>
    CreateModemPayPaymentIntentAsync(
        Guid bookingId,
        CreateModemPayIntentRequestDto request,
        ModemPayCustomerContextDto customerContext,
        CancellationToken cancellationToken = default);
        Task ProcessModemPayWebhookAsync(ModemPayWebhookEvent webhookEvent);
        Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto request);
    }
}
