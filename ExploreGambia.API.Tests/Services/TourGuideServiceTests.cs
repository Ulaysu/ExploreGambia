using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ExploreGambia.API.Tests.Services
{
    public class TourGuideServiceTests
    {
        [Fact]
        public async Task DeleteTourGuideAsync_WhenGuideDoesNotExist_ThrowsNotFound()
        {
            var id = Guid.NewGuid();
            var repository = new Mock<ITourGuideRepository>();
            repository.Setup(repo => repo.GetTourGuideForDeletionAsync(id))
                .ReturnsAsync((TourGuide?)null);
            var service = new TourGuideService(repository.Object);

            await Assert.ThrowsAsync<TourGuideNotFoundException>(
                () => service.DeleteTourGuideAsync(id));

            repository.Verify(repo => repo.DeleteTourGuideAsync(It.IsAny<TourGuide>()), Times.Never);
        }

        [Theory]
        [InlineData(VerificationStatus.PendingReview, EvidenceDeletionStatus.NotRequired, null, null)]
        [InlineData(VerificationStatus.Approved, EvidenceDeletionStatus.Pending, null, null)]
        [InlineData(VerificationStatus.Rejected, EvidenceDeletionStatus.Failed, null, null)]
        [InlineData(VerificationStatus.Approved, EvidenceDeletionStatus.NotRequired, "front-key", null)]
        [InlineData(VerificationStatus.Rejected, EvidenceDeletionStatus.Completed, null, "back-key")]
        public async Task DeleteTourGuideAsync_WhenEvidenceRequiresCleanup_BlocksDeletion(
            VerificationStatus status,
            EvidenceDeletionStatus deletionStatus,
            string? frontKey,
            string? backKey)
        {
            var guide = CreateGuide(new ProviderVerification
            {
                Status = status,
                EvidenceDeletionStatus = deletionStatus,
                TemporaryDocumentFrontKey = frontKey,
                TemporaryDocumentBackKey = backKey
            });
            var repository = CreateRepository(guide);
            var service = new TourGuideService(repository.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => service.DeleteTourGuideAsync(guide.TourGuideId));

            repository.Verify(repo => repo.DeleteTourGuideAsync(It.IsAny<TourGuide>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTourGuideAsync_WhenVerificationIsClean_DeletesGuideAndMetadata()
        {
            var guide = CreateGuide(new ProviderVerification
            {
                Status = VerificationStatus.Approved,
                EvidenceDeletionStatus = EvidenceDeletionStatus.Completed,
                EvidenceDeletedAt = DateTime.UtcNow
            });
            var repository = CreateRepository(guide);
            var service = new TourGuideService(repository.Object);

            var deletedGuide = await service.DeleteTourGuideAsync(guide.TourGuideId);

            Assert.Same(guide, deletedGuide);
            repository.Verify(repo => repo.DeleteTourGuideAsync(guide), Times.Once);
        }

        [Fact]
        public async Task DeleteTourGuideAsync_WhenGuideHasNoVerification_DeletesGuide()
        {
            var guide = CreateGuide();
            var repository = CreateRepository(guide);
            var service = new TourGuideService(repository.Object);

            await service.DeleteTourGuideAsync(guide.TourGuideId);

            repository.Verify(repo => repo.DeleteTourGuideAsync(guide), Times.Once);
        }

        [Fact]
        public async Task DeleteTourGuideAsync_WhenVerificationChangesConcurrently_ReturnsConflict()
        {
            var guide = CreateGuide();
            var repository = CreateRepository(guide);
            repository.Setup(repo => repo.DeleteTourGuideAsync(guide))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            var service = new TourGuideService(repository.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => service.DeleteTourGuideAsync(guide.TourGuideId));
        }

        private static TourGuide CreateGuide(ProviderVerification? verification = null)
        {
            var guide = new TourGuide
            {
                TourGuideId = Guid.NewGuid(),
                FullName = "Deletion Test Guide"
            };

            if (verification != null)
            {
                verification.ProviderVerificationId = Guid.NewGuid();
                verification.TourGuideId = guide.TourGuideId;
                verification.TourGuide = guide;
                guide.Verification = verification;
            }

            return guide;
        }

        private static Mock<ITourGuideRepository> CreateRepository(TourGuide guide)
        {
            var repository = new Mock<ITourGuideRepository>();
            repository.Setup(repo => repo.GetTourGuideForDeletionAsync(guide.TourGuideId))
                .ReturnsAsync(guide);
            repository.Setup(repo => repo.DeleteTourGuideAsync(guide))
                .Returns(Task.CompletedTask);
            return repository;
        }
    }
}
