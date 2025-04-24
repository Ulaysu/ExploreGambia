namespace ExploreGambia.API.Exceptions
{
    public class PaymentNotFoundException : Exception
    {
        public PaymentNotFoundException(Guid paymentId)
            : base($"Payment with ID {paymentId} not found.")
        {
        }
    }
    
}
