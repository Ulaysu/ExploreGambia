namespace ExploreGambia.API.Models.Domain
{
    public enum VerificationStatus
    {
        NotStarted = 0,
        PendingReview = 1,
        Approved = 2,
        Rejected = 3,
        ReverificationRequired = 4
    }
}
