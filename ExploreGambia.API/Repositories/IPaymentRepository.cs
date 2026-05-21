using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<List<Payment>> GetAllPaymentsAsync(string? paymentMethod = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            PaymentStatus? status = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1,
            int pageSize = 10);
        Task<Payment?> GetPaymentById(Guid id);
        Task<List<Payment>> GetBookingsByUserIdAsync(string userId);
        Task<Payment?> GetLatestPaymentByBookingAndMethodAsync(Guid bookingId, string paymentMethod);
        Task<Payment?> GetPaymentByProviderReferenceAsync(string providerReference);
        Task<Payment?> UpdatePaymentAsync(Guid id, Payment payment);
        Task<Payment?> UpdatePaymentStatusAsync(Guid id, PaymentStatus status, string? providerReference = null);
        Task<Payment?> DeletePaymentAsync(Guid id);
    }
}
