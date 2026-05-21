namespace ExploreGambia.API.Services.Payments
{
    public interface IModemPayClient
    {
        Task<ModemPayTransaction?> RetrieveTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
        bool IsValidWebhookSignature(string rawPayload, string? signature);
    }
}
