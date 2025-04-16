namespace ExploreGambia.API.Exceptions
{
    public class TourGuideNotFoundException : Exception
    {
        public TourGuideNotFoundException(Guid tourGuideId)
            :base($"Tour Guide with ID {tourGuideId} not found.")
        {
            
        }
    }
}
