namespace ExploreGambia.API.Models.DTOs
{
    public class PaymentSummaryDto
    {
        public int TotalPayments { get; set; }

        public int SuccessfulPayments { get; set; }

        public int PendingPayments { get; set; }

        public int FailedPayments { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
