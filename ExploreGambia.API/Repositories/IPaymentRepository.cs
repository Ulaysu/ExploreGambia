using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<List<Payment>> GetAllPaymentsAsync(string? paymentMethod = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            bool? isSuccessful = null, string? sortBy = null, bool isAscending = true);
        Task<Payment?> GetPaymentById(Guid id);
        Task<Payment?> UpdatePaymentAsync(Guid id, Payment payment);
        Task<Payment?> DeletePaymentAsync(Guid id);
    }
}
