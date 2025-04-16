using System.Globalization;
using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
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

            if (existingBooking == null) 
            { 
                throw new InvalidOperationException("Booking does not exist.");
            }

            await context.Payments.AddAsync(payment);
            await context.SaveChangesAsync();

            // If payment is successful, update booking status
            if (payment.IsSuccessful)
            {
                existingBooking.Status = BookingStatus.Confirmed;
                await context.SaveChangesAsync();
            }

            return payment;
        }

        // DELETE
        public async Task<Payment?> DeletePaymentAsync(Guid id)
        {
            var existingPayment = await context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);

            if (existingPayment == null) return null;

            context.Payments.Remove(existingPayment);
            await context.SaveChangesAsync();

            return existingPayment;
        }

        // Get all Payments
        public async Task<List<Payment>> GetAllPaymentsAsync(string? paymentMethod = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            bool? isSuccessful = null, string? sortBy = null, bool isAscending = true)
        {
            var payments = context.Payments.Include(p=> p.Booking).AsQueryable();

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

            if (isSuccessful.HasValue)
            {
                payments = payments.Where(p => p.IsSuccessful == isSuccessful.Value);
            }


            // Apply Sorting 
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLowerInvariant())
                {
                    case "paymentdate":
                        payments = isAscending ? payments.OrderBy(p => p.PaymentDate) : payments.OrderByDescending(p => p.PaymentDate);
                        break;
                    case "amount":
                        payments = isAscending ? payments.OrderBy(p => p.Amount) : payments.OrderByDescending(p => p.Amount);
                        break;
                    case "paymentmethod":
                        payments = isAscending ? payments.OrderBy(p => p.PaymentMethod) : payments.OrderByDescending(p => p.PaymentMethod);
                        break;
                    case "issuccessful":
                        payments = isAscending ? payments.OrderBy(p => p.IsSuccessful) : payments.OrderByDescending(p => p.IsSuccessful);
                        break;
                    default:
                        logger.LogWarning($"Received unknown sortBy parameter: '{sortBy}'. No sorting applied to payments.");
                        break;
                }
            }

            return await payments.ToListAsync();
        }

        public async Task<Payment?> GetPaymentById(Guid id)
        {
            var payment = await context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);
            if (payment == null) return null;

            return payment;
        }

        // UPDATE 
        public async Task<Payment?> UpdatePaymentAsync(Guid id, Payment payment)
        {
            var existingPayment = await context.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);

            if (existingPayment == null) return null;

            existingPayment.BookingId = payment.BookingId;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.Amount = payment.Amount;
            existingPayment.PaymentDate = payment.PaymentDate;
            existingPayment.IsSuccessful = payment.IsSuccessful;

            await context.SaveChangesAsync();

            return existingPayment;
        }
    }
}
