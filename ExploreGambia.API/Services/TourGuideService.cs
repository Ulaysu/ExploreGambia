using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Services
{
    public class TourGuideService : ITourGuideService
    {
        private readonly ITourGuideRepository tourGuideRepository;
        private readonly IUnitOfWork unitOfWork;

        public TourGuideService(ITourGuideRepository tourGuideRepository, IUnitOfWork unitOfWork)
        {
            this.tourGuideRepository = tourGuideRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<TourGuide> DeleteTourGuideAsync(Guid id)
        {
            try
            {
                return await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var tourGuide = await tourGuideRepository.GetTourGuideForDeletionAsync(id);

                    if (tourGuide == null)
                    {
                        throw new TourGuideNotFoundException(id);
                    }

                    if (!CanDeleteSafely(tourGuide.Verification))
                    {
                        throw new BusinessRuleException(
                            "Tour guide cannot be deleted until identity evidence cleanup is complete.");
                    }

                    await tourGuideRepository.DeleteTourGuideAsync(tourGuide);
                    return tourGuide;
                });
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new BusinessRuleException(
                    "Tour guide verification changed during deletion. Retry the request.",
                    exception);
            }
        }

        private static bool CanDeleteSafely(ProviderVerification? verification)
        {
            if (verification == null)
            {
                return true;
            }

            var hasNoStorageKeys = verification.TemporaryDocumentFrontKey == null
                && verification.TemporaryDocumentBackKey == null;

            var cleanupWasNotNeeded = verification.Status == VerificationStatus.NotStarted
                && verification.EvidenceDeletionStatus == EvidenceDeletionStatus.NotRequired;
            var cleanupCompleted = verification.EvidenceDeletionStatus == EvidenceDeletionStatus.Completed;

            return hasNoStorageKeys && (cleanupWasNotNeeded || cleanupCompleted);
        }
    }
}
