using AutoMapper;
using ExploreGambia.API.Data;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services.Payments;
using Microsoft.Extensions.Options;

namespace ExploreGambia.API.Services
{
    public class PaymentService : IPaymentService
    {
        private const string ModemPayCardPaymentMethod = "ModemPayCard";
        private readonly ExploreGambiaDbContext dbContext;
        private readonly IBookingRepository bookingRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IModemPayClient modemPayClient;
        private readonly ModemPayOptions modemPayOptions;
        private readonly IMapper mapper;

        public PaymentService(
            ExploreGambiaDbContext dbContext,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IModemPayClient modemPayClient,
            IOptions<ModemPayOptions> modemPayOptions,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.bookingRepository = bookingRepository;
            this.paymentRepository = paymentRepository;
            this.modemPayClient = modemPayClient;
            this.modemPayOptions = modemPayOptions.Value;
            this.mapper = mapper;
        }

        public async Task<Payment> CreatePaymentAsync(AddPaymentRequestDto request)
        {
            var booking = await bookingRepository.GetBookingById(request.BookingId)
                ?? throw new BookingNotFoundException(request.BookingId);

            EnsureBookingCanAcceptPayment(booking);
            EnsureAmountMatchesBooking(request.Amount, booking.TotalAmount);

            var payment = mapper.Map<Payment>(request);
            payment.PaymentId = Guid.NewGuid();
            payment.PaymentDate = DateTime.UtcNow;
            payment.Status = PaymentStatus.Pending;

            return await paymentRepository.CreatePaymentAsync(payment);
        }

        public async Task<Payment> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequestDto request)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var payment = await paymentRepository.GetPaymentById(id)
                ?? throw new PaymentNotFoundException(id);
            var booking = await bookingRepository.GetBookingById(payment.BookingId)
                ?? throw new BookingNotFoundException(payment.BookingId);

            if (payment.Status == PaymentStatus.Succeeded && booking.Status == BookingStatus.Confirmed)
            {
                await transaction.CommitAsync();
                return payment;
            }

            EnsureBookingCanAcceptPayment(booking);
            EnsureAmountMatchesBooking(payment.Amount, booking.TotalAmount);

            await paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, PaymentStatus.Succeeded, request.ProviderReference);
            await bookingRepository.UpdateBookingStatusAsync(booking.BookingId, BookingStatus.Confirmed);

            await transaction.CommitAsync();

            return await paymentRepository.GetPaymentById(id)
                ?? throw new PaymentNotFoundException(id);
        }

        public async Task<Payment> ConfirmProviderPaymentAsync(Guid id, string? providerReference)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var payment = await paymentRepository.GetPaymentById(id)
                ?? throw new PaymentNotFoundException(id);
            var booking = await bookingRepository.GetBookingById(payment.BookingId)
                ?? throw new BookingNotFoundException(payment.BookingId);

            if (payment.Status == PaymentStatus.Succeeded && booking.Status == BookingStatus.Confirmed)
            {
                await transaction.CommitAsync();
                return payment;
            }

            EnsureBookingCanAcceptPayment(booking);
            EnsureAmountMatchesBooking(payment.Amount, booking.TotalAmount);

            await paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, PaymentStatus.Succeeded, providerReference);
            await bookingRepository.UpdateBookingStatusAsync(booking.BookingId, BookingStatus.Confirmed);

            await transaction.CommitAsync();

            return await paymentRepository.GetPaymentById(id)
                ?? throw new PaymentNotFoundException(id);
        }

        public async Task<ModemPayInlinePaymentResponseDto> PrepareModemPayInlinePaymentAsync(
            Guid bookingId,
            ModemPayInlinePaymentRequestDto request,
            ModemPayCustomerContextDto customerContext)
        {
            EnsureModemPayConfigured(requireSecretKey: false);

            var booking = await bookingRepository.GetBookingById(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (!customerContext.IsAdmin && booking.UserId != customerContext.UserId)
            {
                throw new UnauthorizedAccessException("You cannot pay for another user's booking.");
            }

            EnsureBookingCanAcceptPayment(booking);

            var payment = await paymentRepository.GetLatestPaymentByBookingAndMethodAsync(booking.BookingId, ModemPayCardPaymentMethod);
            if (payment == null || payment.Status is PaymentStatus.Failed or PaymentStatus.Canceled)
            {
                payment = await paymentRepository.CreatePaymentAsync(new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    BookingId = booking.BookingId,
                    PaymentMethod = ModemPayCardPaymentMethod,
                    Amount = booking.TotalAmount,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending
                });
            }
            else if (payment.Status == PaymentStatus.Succeeded)
            {
                throw new BusinessRuleException("This booking has already been paid.");
            }

            var customerName = FirstNonEmpty(request.CustomerName, customerContext.Name);
            var customerEmail = FirstNonEmpty(request.CustomerEmail, customerContext.Email);

            return new ModemPayInlinePaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                BookingId = booking.BookingId,
                Amount = payment.Amount,
                Currency = modemPayOptions.Currency,
                PublicKey = modemPayOptions.PublicKey,
                PaymentMethods = "card",
                Title = "ExploreGambia tour booking",
                Description = $"Payment for booking {booking.BookingId}",
                Customer = customerContext.UserId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = request.CustomerPhone,
                Metadata = new Dictionary<string, string>
                {
                    ["bookingId"] = booking.BookingId.ToString(),
                    ["paymentId"] = payment.PaymentId.ToString(),
                    ["provider"] = "ModemPay"
                }
            };
        }

        public async Task<Payment> VerifyModemPayPaymentAsync(
            VerifyModemPayPaymentRequestDto request,
            ModemPayCustomerContextDto customerContext,
            CancellationToken cancellationToken = default)
        {
            EnsureModemPayConfigured(requireSecretKey: true);

            var transaction = await modemPayClient.RetrieveTransactionAsync(request.TransactionId, cancellationToken)
                ?? throw new BusinessRuleException("Could not verify the Modem Pay transaction.");

            return await ProcessModemPayTransactionAsync(transaction, customerContext);
        }

        public async Task ProcessModemPayWebhookAsync(ModemPayWebhookEvent webhookEvent)
        {
            var eventName = webhookEvent.Event.Trim().ToLowerInvariant();

            switch (eventName)
            {
                case "charge.succeeded":
                    await ProcessModemPayTransactionAsync(webhookEvent.Payload);
                    break;

                case "charge.failed":
                case "charge.cancelled":
                case "charge.canceled":
                case "charge.expired":
                case "payment_intent.cancelled":
                case "payment_intent.canceled":
                case "payment_intent.expired":
                    await MarkModemPayTransactionIncompleteAsync(webhookEvent.Payload, MapIncompleteStatus(eventName));
                    break;
            }
        }

        public async Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto request)
        {
            var booking = await bookingRepository.GetBookingById(request.BookingId)
                ?? throw new BookingNotFoundException(request.BookingId);

            EnsureBookingCanAcceptPayment(booking);
            EnsureAmountMatchesBooking(request.Amount, booking.TotalAmount);

            var payment = mapper.Map<Payment>(request);
            return await paymentRepository.UpdatePaymentAsync(id, payment);
        }

        private async Task<Payment> ProcessModemPayTransactionAsync(
            ModemPayTransaction transaction,
            ModemPayCustomerContextDto? customerContext = null)
        {
            if (!IsCompleted(transaction.Status))
            {
                throw new BusinessRuleException("Modem Pay transaction is not completed.");
            }

            var payment = await ResolveModemPayPaymentAsync(transaction);
            var booking = await bookingRepository.GetBookingById(payment.BookingId)
                ?? throw new BookingNotFoundException(payment.BookingId);

            if (customerContext != null && !customerContext.IsAdmin && booking.UserId != customerContext.UserId)
            {
                throw new UnauthorizedAccessException("You cannot verify another user's payment.");
            }

            EnsureAmountMatchesBooking(transaction.Amount, payment.Amount);
            EnsureAmountMatchesBooking(payment.Amount, booking.TotalAmount);

            if (!string.Equals(transaction.Currency, modemPayOptions.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("Modem Pay transaction currency does not match the booking currency.");
            }

            return await ConfirmProviderPaymentAsync(payment.PaymentId, GetProviderReference(transaction));
        }

        private async Task MarkModemPayTransactionIncompleteAsync(ModemPayTransaction transaction, PaymentStatus status)
        {
            var payment = await ResolveModemPayPaymentAsync(transaction);
            if (payment.Status == PaymentStatus.Succeeded)
            {
                return;
            }

            await paymentRepository.UpdatePaymentStatusAsync(payment.PaymentId, status, GetProviderReference(transaction));
        }

        private async Task<Payment> ResolveModemPayPaymentAsync(ModemPayTransaction transaction)
        {
            var paymentId = GetMetadataValue(transaction, "paymentId");
            if (Guid.TryParse(paymentId, out var parsedPaymentId))
            {
                return await paymentRepository.GetPaymentById(parsedPaymentId)
                    ?? throw new PaymentNotFoundException(parsedPaymentId);
            }

            var providerReference = GetProviderReference(transaction);
            if (!string.IsNullOrWhiteSpace(providerReference))
            {
                var payment = await paymentRepository.GetPaymentByProviderReferenceAsync(providerReference);
                if (payment != null)
                {
                    return payment;
                }
            }

            throw new BusinessRuleException("Modem Pay transaction does not reference a known payment.");
        }

        private void EnsureModemPayConfigured(bool requireSecretKey)
        {
            if (string.IsNullOrWhiteSpace(modemPayOptions.PublicKey))
            {
                throw new InvalidOperationException("Modem Pay public key is not configured.");
            }

            if (requireSecretKey && string.IsNullOrWhiteSpace(modemPayOptions.SecretKey))
            {
                throw new InvalidOperationException("Modem Pay secret key is not configured.");
            }
        }

        public async Task<ModemPayPaymentIntentResponseDto>
    CreateModemPayPaymentIntentAsync(
        Guid bookingId,
        CreateModemPayIntentRequestDto request,
        ModemPayCustomerContextDto customerContext,
        CancellationToken cancellationToken = default)
        {
            EnsureModemPayConfigured(true);

            var booking =
                await bookingRepository.GetBookingById(bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (!customerContext.IsAdmin &&
                booking.UserId != customerContext.UserId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot pay for another user's booking.");
            }

            EnsureBookingCanAcceptPayment(booking);

            var payment =
                await paymentRepository
                .GetLatestPaymentByBookingAndMethodAsync(
                    booking.BookingId,
                    ModemPayCardPaymentMethod);

            if (payment == null ||
                payment.Status == PaymentStatus.Failed ||
                payment.Status == PaymentStatus.Canceled)
            {
                payment = await paymentRepository.CreatePaymentAsync(
                    new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        BookingId = booking.BookingId,
                        Amount = booking.TotalAmount,
                        PaymentDate = DateTime.UtcNow,
                        PaymentMethod = ModemPayCardPaymentMethod,
                        Status = PaymentStatus.Pending
                    });
            }

            if (payment.Status == PaymentStatus.Succeeded)
            {
                throw new BusinessRuleException(
                    "Booking already paid.");
            }

            var modemPayRequest =
                new ModemPayPaymentInentRequestDto
                {
                    Amount = booking.TotalAmount,
                    Currency = modemPayOptions.Currency,

                    ReturnUrl = request.ReturnUrl,
                    CancelUrl = request.CancelUrl,
                    PaymentMethods = new List<string>
                    {
                        "card"
                    },

                    Metadata = new Dictionary<string, string>
                    {
                        ["paymentId"] = payment.PaymentId.ToString(),
                        ["bookingId"] = booking.BookingId.ToString(),
                        ["userId"] = customerContext.UserId
                    }
                };

            var response =
                await modemPayClient.CreatePaymentIntentAsync(
                    modemPayRequest,
                    cancellationToken);

            if (response == null)
            {
                throw new BusinessRuleException(
                    "Failed to create ModemPay payment intent.");
            }

            return response;
        }

        private static string? GetProviderReference(ModemPayTransaction transaction)
        {
            return FirstNonEmpty(transaction.Id, transaction.TransactionReference, transaction.PaymentIntentId);
        }

        private static string? GetMetadataValue(ModemPayTransaction transaction, string key)
        {
            if (transaction.Metadata == null)
            {
                return null;
            }

            var match = transaction.Metadata.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            return match.Value.ValueKind == System.Text.Json.JsonValueKind.String
                ? match.Value.GetString()
                : match.Value.ToString();
        }

        private static bool IsCompleted(string? status)
        {
            return string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
        }

        private static PaymentStatus MapIncompleteStatus(string eventName)
        {
            return eventName.Contains("cancel", StringComparison.OrdinalIgnoreCase)
                ? PaymentStatus.Canceled
                : PaymentStatus.Failed;
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        private static void EnsureBookingCanAcceptPayment(Booking booking)
        {
            if (booking.Status == BookingStatus.Canceled || booking.Status == BookingStatus.Completed)
            {
                throw new BusinessRuleException($"Booking with status '{booking.Status}' cannot accept payment.");
            }

            if (booking.Status == BookingStatus.Confirmed)
            {
                throw new BusinessRuleException("This booking has already been confirmed.");
            }
        }

        private static void EnsureAmountMatchesBooking(decimal paymentAmount, decimal bookingTotal)
        {
            if (paymentAmount != bookingTotal)
            {
                throw new BusinessRuleException("Payment amount does not match the booking total.");
            }
        }
    }
}
