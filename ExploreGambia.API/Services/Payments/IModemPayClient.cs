using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services.Payments
{
    public interface IModemPayClient
    {
        Task<ModemPayTransaction?> RetrieveTransactionAsync(string transactionId, CancellationToken cancellationToken = default);

        Task<ModemPayPaymentIntentResponseDto?> CreatePaymentIntentAsync(
    ModemPayPaymentInentRequestDto request,
    CancellationToken cancellationToken = default);

        bool IsValidWebhookSignature(string rawPayload, string? signature);
    }
}
