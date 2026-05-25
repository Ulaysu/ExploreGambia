namespace ExploreGambia.API.Services.Payments
{
    public class ModemPayOptions
    {
        public string PublicKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Currency { get; set; } = "GMD";
        public string BaseUrl { get; set; } = "https://api.modempay.com";
        public string TransactionPathTemplate { get; set; } = "/v1/transactions/{transactionId}";
    }
}
