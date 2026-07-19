using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Services
{
    public class TourGuideService : ITourGuideService
    {
        private readonly ITourGuideRepository tourGuideRepository;

        public TourGuideService(ITourGuideRepository tourGuideRepository)
        {
            this.tourGuideRepository = tourGuideRepository;
        }

        public async Task<TourGuide> DeleteTourGuideAsync(Guid id)
        {
            var tourGuide = await tourGuideRepository.GetTourGuideForDeletionAsync(id);

            if (tourGuide == null)
            {
                throw new TourGuideNotFoundException(id);
            }

            if (RequiresEvidenceCleanup(tourGuide.Verification))
            {
                throw new BusinessRuleException(
                    "Tour guide cannot be deleted until identity evidence cleanup is complete.");
            }

            try
            {
                await tourGuideRepository.DeleteTourGuideAsync(tourGuide);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessRuleException(
                    "Tour guide verification changed during deletion. Retry the request.");
            }

            return tourGuide;
        }

        private static bool RequiresEvidenceCleanup(ProviderVerification? verification)
        {
            if (verification == null)
            {
                return false;
            }

            return verification.Status == VerificationStatus.PendingReview
                || verification.TemporaryDocumentFrontKey != null
                || verification.TemporaryDocumentBackKey != null
                || verification.EvidenceDeletionStatus is EvidenceDeletionStatus.Pending
                    or EvidenceDeletionStatus.Failed;
        }
    }
}
