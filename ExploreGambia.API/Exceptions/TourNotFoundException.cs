namespace ExploreGambia.API.Exceptions
{
    public class TourNotFoundException : Exception
    {
        public TourNotFoundException(Guid tourId)
            :base($"Tour with ID {tourId} not found.")
        {
            
        }
    }
}
