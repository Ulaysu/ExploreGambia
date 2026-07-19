using System.Net;
using System.Net.Http.Json;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ExploreGambia.API.Tests.Authorization
{
    [Collection(IntegrationTestCollection.Name)]
    public class GuideAuthorizationIntegrationTests
    {
        private static readonly Guid GuideId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid TourId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        public static IEnumerable<object[]> GuideOnlyEndpoints()
        {
            yield return new object[] { HttpMethod.Get, "/api/v1/tour-guides/me" };
            yield return new object[] { HttpMethod.Put, "/api/v1/tour-guides/me" };
            yield return new object[] { HttpMethod.Post, "/api/v1/tours" };
            yield return new object[] { HttpMethod.Put, $"/api/v1/tours/{TourId}" };
            yield return new object[] { HttpMethod.Patch, $"/api/v1/tours/{TourId}/availability" };
            yield return new object[] { HttpMethod.Delete, $"/api/v1/tours/{TourId}" };
            yield return new object[] { HttpMethod.Get, $"/api/v1/tours/{TourId}/participants" };
            yield return new object[] { HttpMethod.Get, $"/api/v1/tours/my/{TourId}" };
            yield return new object[] { HttpMethod.Get, "/api/v1/tours/my" };
        }

        public static IEnumerable<object[]> GuideReadEndpoints()
        {
            yield return new object[] { "/api/v1/tour-guides/me" };
            yield return new object[] { $"/api/v1/tours/{TourId}/participants" };
            yield return new object[] { $"/api/v1/tours/my/{TourId}" };
            yield return new object[] { "/api/v1/tours/my" };
        }

        public static IEnumerable<object[]> AdminOnlyTourGuideManagementEndpoints()
        {
            yield return new object[] { HttpMethod.Post, "/api/v1/tour-guides" };
            yield return new object[] { HttpMethod.Put, $"/api/v1/tour-guides/{GuideId}" };
            yield return new object[] { HttpMethod.Delete, $"/api/v1/tour-guides/{GuideId}" };
        }

        [Theory]
        [MemberData(nameof(GuideOnlyEndpoints))]
        public async Task GuideOnlyEndpoints_RejectAnonymousRequests(HttpMethod method, string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateClient();

            var response = await client.SendAsync(CreateGuideRequest(method, url));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(GuideOnlyEndpoints))]
        public async Task GuideOnlyEndpoints_RejectUserRole(HttpMethod method, string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.UserRole);

            var response = await client.SendAsync(CreateGuideRequest(method, url));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(GuideReadEndpoints))]
        public async Task GuideReadEndpoints_AllowGuideRole(string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.GuideRole);

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AdminOnlyTourGuideManagementEndpoints))]
        public async Task TourGuideManagementEndpoints_RejectNonAdminRoles(HttpMethod method, string url)
        {
            using var factory = CreateFactory();

            using var userClient = factory.CreateAuthenticatedClient(TestUsers.UserRole);
            using var userResponse = await userClient.SendAsync(CreateTourGuideManagementRequest(method, url));

            using var guideClient = factory.CreateAuthenticatedClient(TestUsers.GuideRole);
            using var guideResponse = await guideClient.SendAsync(CreateTourGuideManagementRequest(method, url));

            Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, guideResponse.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AdminOnlyTourGuideManagementEndpoints))]
        public async Task TourGuideManagementEndpoints_AllowAdminRole(HttpMethod method, string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.AdminRole);

            var response = await client.SendAsync(CreateTourGuideManagementRequest(method, url));

            Assert.True(
                response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
                $"Expected OK or Created, got {response.StatusCode}.");
        }

        private static AuthorizationTestWebApplicationFactory CreateFactory()
        {
            return new AuthorizationTestWebApplicationFactory(services =>
            {
                services.RemoveAll<ITourGuideRepository>();
                services.RemoveAll<ITourRepository>();
                services.RemoveAll<IUnitOfWork>();

                services.AddSingleton(CreateTourGuideRepository().Object);
                services.AddSingleton(CreateTourRepository().Object);
                services.AddSingleton(CreateUnitOfWork().Object);
            });
        }

        private static HttpRequestMessage CreateGuideRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Post)
            {
                request.Content = JsonContent.Create(CreateAddTourRequest());
            }
            else if (method == HttpMethod.Put && url.Contains("/tour-guides/me", StringComparison.OrdinalIgnoreCase))
            {
                request.Content = JsonContent.Create(new UpdateTourGuideProfileDto
                {
                    PhoneNumber = "+220 555 0000",
                    Bio = "Experienced authorization test guide.",
                    IsAvailable = true
                });
            }
            else if (method == HttpMethod.Put)
            {
                request.Content = JsonContent.Create(CreateUpdateTourRequest());
            }
            else if (method == HttpMethod.Patch)
            {
                request.Content = JsonContent.Create(new UpdateTourAvailabilityDto
                {
                    IsAvailable = true
                });
            }

            return request;
        }

        private static HttpRequestMessage CreateTourGuideManagementRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Post)
            {
                request.Content = JsonContent.Create(new AddTourGuideRequestDto
                {
                    FullName = "Admin Managed Guide",
                    PhoneNumber = "+220 555 0101",
                    Email = "managed-guide@example.test",
                    Bio = "Created by authorization tests.",
                    IsAvailable = true
                });
            }
            else if (method == HttpMethod.Put)
            {
                request.Content = JsonContent.Create(new UpdateTourGuideRequestDto
                {
                    FullName = "Updated Guide",
                    PhoneNumber = "+220 555 0202",
                    Email = "updated-guide@example.test",
                    Bio = "Updated by authorization tests.",
                    IsAvailable = true
                });
            }

            return request;
        }

        private static AddTourRequestDto CreateAddTourRequest()
        {
            return new AddTourRequestDto
            {
                Title = "Guide Test Tour",
                Description = "A tour used by authorization integration tests.",
                Location = "Banjul",
                Price = 25m,
                MaxParticipants = 8,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsAvailable = true
            };
        }

        private static UpdateTourRequestDto CreateUpdateTourRequest()
        {
            return new UpdateTourRequestDto
            {
                Title = "Updated Guide Test Tour",
                Description = "An updated tour used by authorization integration tests.",
                Location = "Serrekunda",
                Price = 30m,
                MaxParticipants = 10,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsAvailable = true
            };
        }

        private static Mock<ITourGuideRepository> CreateTourGuideRepository()
        {
            var guide = CreateGuide();
            var repository = new Mock<ITourGuideRepository>(MockBehavior.Strict);

            repository
                .Setup(repo => repo.GetTourGuideByUserIdAsync(TestUsers.GuideId))
                .ReturnsAsync(guide);
            repository
                .Setup(repo => repo.CreateTourGuideAsync(It.IsAny<TourGuide>()))
                .ReturnsAsync((TourGuide tourGuide) =>
                {
                    tourGuide.TourGuideId = GuideId;
                    return tourGuide;
                });
            repository
                .Setup(repo => repo.UpdateTourGuideAsync(GuideId, It.IsAny<TourGuide>()))
                .ReturnsAsync((Guid _, TourGuide tourGuide) =>
                {
                    tourGuide.TourGuideId = GuideId;
                    return tourGuide;
                });
            repository
                .Setup(repo => repo.GetTourGuideForDeletionAsync(GuideId))
                .ReturnsAsync(guide);
            repository
                .Setup(repo => repo.DeleteTourGuideAsync(guide))
                .Returns(Task.CompletedTask);

            return repository;
        }

        private static Mock<IUnitOfWork> CreateUnitOfWork()
        {
            var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWork
                .Setup(candidate => candidate.ExecuteInTransactionAsync(It.IsAny<Func<Task<TourGuide>>>()))
                .Returns((Func<Task<TourGuide>> operation) => operation());
            return unitOfWork;
        }

        private static Mock<ITourRepository> CreateTourRepository()
        {
            var tour = CreateTour();
            var repository = new Mock<ITourRepository>(MockBehavior.Strict);

            repository.Setup(repo => repo.GetTourById(TourId)).ReturnsAsync(tour);
            repository.Setup(repo => repo.GetTourByIdAndUserIdAsync(TourId, TestUsers.GuideId)).ReturnsAsync(tour);
            repository.Setup(repo => repo.GetParticipantsAsync(TourId)).ReturnsAsync(new List<TourParticipantDto>());
            repository.Setup(repo => repo.GetToursByUserIdAsync(TestUsers.GuideId)).ReturnsAsync(new List<Tour> { tour });
            repository.Setup(repo => repo.UpdateAvailabilityAsync(TourId, true)).ReturnsAsync(tour);
            repository.Setup(repo => repo.CreateTourAsync(It.IsAny<Tour>())).ReturnsAsync((Tour createdTour) =>
            {
                createdTour.TourId = TourId;
                createdTour.TourGuide = CreateGuide();
                return createdTour;
            });
            repository.Setup(repo => repo.UpdateTourAsync(TourId, It.IsAny<Tour>())).ReturnsAsync((Guid _, Tour updatedTour) =>
            {
                updatedTour.TourId = TourId;
                updatedTour.TourGuide = CreateGuide();
                return updatedTour;
            });
            repository.Setup(repo => repo.DeleteTourAsync(TourId, GuideId)).ReturnsAsync(tour);

            return repository;
        }

        private static TourGuide CreateGuide()
        {
            return new TourGuide
            {
                TourGuideId = GuideId,
                UserId = TestUsers.GuideId,
                FullName = "Authorization Guide",
                PhoneNumber = "+220 555 0000",
                Email = TestUsers.GuideEmail,
                Bio = "Guide used by authorization integration tests.",
                IsAvailable = true
            };
        }

        private static Tour CreateTour()
        {
            var guide = CreateGuide();

            return new Tour
            {
                TourId = TourId,
                TourGuideId = guide.TourGuideId,
                TourGuide = guide,
                Title = "Guide Test Tour",
                Description = "A tour used by authorization integration tests.",
                Location = "Banjul",
                Price = 25m,
                MaxParticipants = 8,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsAvailable = true
            };
        }
    }
}
