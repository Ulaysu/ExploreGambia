using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ExploreGambiaDbContext context;

        public PaymentRepository(ExploreGambiaDbContext context)
        {
            this.context = context;
        }

        // CREATE 
        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            await context.Payments.AddAsync(payment);
            await context.SaveChangesAsync();

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
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await context.Payments.ToListAsync();
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
