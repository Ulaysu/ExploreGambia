namespace ExploreGambia.API.Models.Domain
{
    public class ProviderVerification
    {
        public Guid ProviderVerificationId { get; set; }

        public Guid TourGuideId { get; set; }

        public TourGuide TourGuide { get; set; } = null!;

        public VerificationStatus Status { get; set; } = VerificationStatus.NotStarted;

        public string? DocumentType { get; set; }

        public string? IssuingCountry { get; set; }

        public string? MaskedDocumentNumber { get; set; }

        public DateOnly? DocumentExpiryDate { get; set; }

        public string? TemporaryDocumentFrontKey { get; set; }

        public string? TemporaryDocumentBackKey { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedByUserId { get; set; }

        public string? ReviewReason { get; set; }

        public EvidenceDeletionStatus EvidenceDeletionStatus { get; set; } = EvidenceDeletionStatus.NotRequired;

        public int EvidenceDeletionAttempts { get; set; }

        public DateTime? EvidenceDeletedAt { get; set; }

        public DateTime? LastEvidenceDeletionAttemptAt { get; set; }

        public string? LastEvidenceDeletionError { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
