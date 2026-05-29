using ExploreGambia.API.Models.Configurations;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ExploreGambia.API.Services.Payments
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentService _paymentService;
        private readonly StripeOptions _stripeOptions;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IBookingRepository bookingRepository, 
            IPaymentRepository paymentRepository,
            IPaymentService paymentService,
            IOptions<StripeOptions> stripeOptions,
            ILogger<StripePaymentService> logger)
        {
            this._bookingRepository = bookingRepository;
            this._paymentRepository = paymentRepository;
            this._paymentService = paymentService;
            this._stripeOptions = stripeOptions.Value;
            this._logger = logger;
        }

        public async Task<StripeCheckoutResponseDto>
            CreateCheckoutSessionAsync(
                Guid bookingId,
                string userId,
                CreateStripeCheckoutRequestDto request)
        {
            var booking =
                await _bookingRepository.GetBookingById(bookingId)
                ?? throw new Exception("Booking not found.");

            if (booking.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot pay for another user's booking.");
            }

            if (booking.Status == BookingStatus.Confirmed)
            {
                throw new Exception("Booking already paid.");
            }

            var payment =
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    BookingId = booking.BookingId,
                    Amount = booking.TotalAmount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "Stripe",
                    Status = PaymentStatus.Pending
                };

            await _paymentRepository.CreatePaymentAsync(payment);

            var options =
                new SessionCreateOptions
                {
                    Mode = "payment",

                    SuccessUrl =
                        request.SuccessUrl
                        ?? _stripeOptions.SuccessUrl,

                    CancelUrl =
                        request.CancelUrl
                        ?? _stripeOptions.CancelUrl,
                    Metadata =
                    new Dictionary<string, string>
                    {
                        ["paymentId"] =
                            payment.PaymentId.ToString(),

                        ["bookingId"] =
                            booking.BookingId.ToString(),

                        ["userId"] = userId
                    },

                    LineItems =
                    new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Quantity = 1,

                            PriceData =
                                new SessionLineItemPriceDataOptions
                                {
                                    Currency = "usd",

                                    UnitAmount =
                                        (long)(booking.TotalAmount * 100),

                                    ProductData =
                                        new SessionLineItemPriceDataProductDataOptions
                                        {
                                            Name =
                                                booking.Tour.Title
                                        }
                                }
                        }
                    }
                };

            var service = new SessionService();

            var session =
                await service.CreateAsync(options);

            payment.ProviderReference = session.Id;

            await _paymentRepository.UpdatePaymentAsync(payment.PaymentId,payment);

            return new StripeCheckoutResponseDto
            {
                CheckoutUrl = session.Url,
                SessionId = session.Id
            };
        }

        public Task HandleStripeWebhookAsync(string json, string? stripeSignature)
        {
            if (string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret))
            {
                throw new InvalidOperationException("Stripe webhook secret is not configured.");
            }

            var stripeEvent =
                EventUtility.ConstructEvent(
                    json,
                    stripeSignature ?? string.Empty,
                    _stripeOptions.WebhookSecret);

            _logger.LogInformation(
                "Verified Stripe webhook event {EventType}.",
                stripeEvent.Type);

            return Task.CompletedTask;
        }
    }
}
