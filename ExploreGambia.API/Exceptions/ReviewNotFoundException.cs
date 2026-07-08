namespace ExploreGambia.API.Exceptions
{
    public class ReviewNotFoundException : Exception
    {
        public ReviewNotFoundException(Guid reviewId)
            : base($"Review with ID {reviewId} not found.")
        {
        }
    }
}
