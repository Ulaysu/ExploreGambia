using AutoMapper;
using ExploreGambia.API.Data;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;

namespace ExploreGambia.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ExploreGambiaDbContext dbContext;
        private readonly IBookingRepository bookingRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IMapper mapper;

        public PaymentService(
            ExploreGambiaDbContext dbContext,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.bookingRepository = bookingRepository;
            this.paymentRepository = paymentRepository;
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

        public async Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto request)
        {
            var booking = await bookingRepository.GetBookingById(request.BookingId)
                ?? throw new BookingNotFoundException(request.BookingId);

            EnsureBookingCanAcceptPayment(booking);
            EnsureAmountMatchesBooking(request.Amount, booking.TotalAmount);

            var payment = mapper.Map<Payment>(request);
            return await paymentRepository.UpdatePaymentAsync(id, payment);
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
