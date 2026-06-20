using ExploreGambia.API.Data;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExploreGambia.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private const int MaxPageSize = 10;
        private readonly ExploreGambiaDbContext context;
        private readonly ILogger<PaymentRepository> logger;

        public PaymentRepository(ExploreGambiaDbContext context, ILogger<PaymentRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        // CREATE 
        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            var existingBooking = await context.Bookings.FirstOrDefaultAsync(x => x.BookingId == payment.BookingId);

            if (existingBooking == null) throw new BookingNotFoundException(payment.BookingId);

            await context.Payments.AddAsync(payment);
            await context.SaveChangesAsync();

            return payment;
        }

        // DELETE
        public async Task<Payment?> DeletePaymentAsync(Guid id)
        {
            var existingPayment = await context.Payments.Include(p => p.Booking).ThenInclude(b => b.Tour).FirstOrDefaultAsync(x => x.PaymentId == id);

            if (existingPayment == null) throw new PaymentNotFoundException(id);

            context.Payments.Remove(existingPayment);
            await context.SaveChangesAsync();

            return existingPayment;
        }

        // Get all Payments
        public async Task<List<Payment>> GetAllPaymentsAsync(string? paymentMethod = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            PaymentStatus? status = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1,
            int pageSize = 10)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var payments = context.Payments
                .AsNoTracking()
                .Include(p=> p.Booking)
                .ThenInclude(b => b.Tour)
                .AsQueryable();

            // Apply Filtering
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                string pattern = $"%{paymentMethod}%";
                payments = payments.Where(p => EF.Functions.Like(p.PaymentMethod, pattern));
            }

            if (paymentDateFrom.HasValue)
            {
                payments = payments.Where(p => p.PaymentDate >= paymentDateFrom.Value);
            }

            if (paymentDateTo.HasValue)
            {
                payments = payments.Where(p => p.PaymentDate <= paymentDateTo.Value.AddDays(1).AddTicks(-1)); // Inclusive
            }

            if (status.HasValue)
            {
                payments = payments.Where(p => p.Status == status.Value);
            }

            var isSorted = false;

            // Apply Sorting 
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLowerInvariant())
                {
                    case "paymentdate":
                        payments = isAscending ? payments.OrderBy(p => p.PaymentDate) : payments.OrderByDescending(p => p.PaymentDate);
                        isSorted = true;
                        break;
                    case "amount":
                        payments = isAscending ? payments.OrderBy(p => p.Amount) : payments.OrderByDescending(p => p.Amount);
                        isSorted = true;
                        break;
                    case "paymentmethod":
                        payments = isAscending ? payments.OrderBy(p => p.PaymentMethod) : payments.OrderByDescending(p => p.PaymentMethod);
                        isSorted = true;
                        break;
                    case "status":
                        payments = isAscending ? payments.OrderBy(p => p.Status) : payments.OrderByDescending(p => p.Status);
                        isSorted = true;
                        break;
                    default:
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied to payments.");
                        break;
                }
            }

            if (!isSorted)
            {
                payments = payments.OrderBy(p => p.PaymentId);
            }

            return await payments.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public Task<List<Payment>> GetBookingsByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<Payment?> GetPaymentById(Guid id)
        {
            var payment = await context.Payments
                .AsNoTracking()
                .Include(p => p.Booking)
                .ThenInclude(b => b.Tour)
                .FirstOrDefaultAsync(x => x.PaymentId == id);
            if (payment == null) throw new PaymentNotFoundException(id);

            return payment;
        }

        public async Task<Payment?> GetLatestPaymentByBookingAndMethodAsync(Guid bookingId, string paymentMethod)
        {
            return await context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Tour)
                .Where(p => p.BookingId == bookingId && p.PaymentMethod == paymentMethod)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Payment?> GetPaymentByProviderReferenceAsync(string providerReference)
        {
            return await context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Tour)
                .FirstOrDefaultAsync(p => p.ProviderReference == providerReference);
        }

        // UPDATE 
        public async Task<Payment?> UpdatePaymentAsync(Guid id, Payment payment)
        {
            var existingPayment = await context.Payments.Include(p => p.Booking).ThenInclude(b => b.Tour).FirstOrDefaultAsync(x => x.PaymentId == id);

            if (existingPayment == null) throw new PaymentNotFoundException(id);

            existingPayment.BookingId = payment.BookingId;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.Amount = payment.Amount;
            existingPayment.ProviderReference = payment.ProviderReference;

            await context.SaveChangesAsync();

            return existingPayment;
        }

        public async Task<Payment?> UpdatePaymentStatusAsync(Guid id, PaymentStatus status, string? providerReference = null)
        {
            var existingPayment = await context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);

            if (existingPayment == null) throw new PaymentNotFoundException(id);

            existingPayment.Status = status;
            existingPayment.ProviderReference = providerReference ?? existingPayment.ProviderReference;
            existingPayment.PaymentDate = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return existingPayment;
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync()
        {
            return new PaymentSummaryDto
            {
                TotalPayments =
                    await context.Payments.CountAsync(),

                SuccessfulPayments =
                    await context.Payments.CountAsync(
                        p => p.Status == PaymentStatus.Succeeded),

                PendingPayments =
                    await context.Payments.CountAsync(
                        p => p.Status == PaymentStatus.Pending),

                FailedPayments =
                    await context.Payments.CountAsync(
                        p => p.Status == PaymentStatus.Failed),

                TotalRevenue =
                    await context.Payments
                        .Where(p => p.Status == PaymentStatus.Succeeded)
                        .SumAsync(p => p.Amount)
            };
        }
    }
}
