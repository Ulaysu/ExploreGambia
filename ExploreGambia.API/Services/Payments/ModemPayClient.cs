using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.DTOs;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExploreGambia.API.Services.Payments
{
    public class ModemPayClient : IModemPayClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient httpClient;
        private readonly ModemPayOptions options;

        public ModemPayClient(HttpClient httpClient, IOptions<ModemPayOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        public async Task<ModemPayTransaction?> RetrieveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            var path = options.TransactionPathTemplate.Replace("{transactionId}", Uri.EscapeDataString(transactionId));
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.SecretKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<ModemPayTransaction>(stream, JsonOptions, cancellationToken);
        }

        public bool IsValidWebhookSignature(string rawPayload, string? signature)
        {
            if (string.IsNullOrWhiteSpace(options.WebhookSecret) || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.WebhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawPayload));
            var hex = Convert.ToHexString(hash).ToLowerInvariant();
            var base64 = Convert.ToBase64String(hash);
            var normalizedSignature = signature.Trim();

            return FixedTimeEquals(normalizedSignature, hex)
                || FixedTimeEquals(normalizedSignature, $"sha256={hex}")
                || FixedTimeEquals(normalizedSignature, base64)
                || FixedTimeEquals(normalizedSignature, $"sha256={base64}");
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            return leftBytes.Length == rightBytes.Length
                && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        

public async Task<ModemPayPaymentIntentResponseDto>
    CreatePaymentIntentAsync(
        ModemPayPaymentInentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var payload =
            new ModemPayCreatePaymentWrapperDto
            {
                Data = request
            };

        var json =
            JsonSerializer.Serialize(payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await httpClient.PostAsync(
                "/v1/payments",
                content,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new BusinessRuleException(
                $"ModemPay error: {responseContent}");
        }

        var options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        var apiResponse =
            JsonSerializer.Deserialize<ModemPayApiResponseDto>(
                responseContent,
                options);

        if (apiResponse == null)
        {
            throw new BusinessRuleException(
                "Unable to deserialize ModemPay response.");
        }

        if (!apiResponse.Status)
        {
            throw new BusinessRuleException(
                apiResponse.Message);
        }

        return apiResponse.Data;
    }
    

}
}
