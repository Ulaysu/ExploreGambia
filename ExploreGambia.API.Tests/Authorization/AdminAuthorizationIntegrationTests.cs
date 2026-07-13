using System.Net;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ExploreGambia.API.Tests.Authorization
{
    public class AdminAuthorizationIntegrationTests
    {
        public static IEnumerable<object[]> AdminEndpoints()
        {
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/dashboard" };
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/users" };
            yield return new object[] { HttpMethod.Get, $"/api/v1/admin/users/{TestUsers.UserId}" };
            yield return new object[] { HttpMethod.Put, $"/api/v1/admin/users/{TestUsers.UserId}/status" };
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/tours" };
            yield return new object[] { HttpMethod.Patch, $"/api/v1/admin/tours/{Guid.NewGuid()}/delete" };
            yield return new object[] { HttpMethod.Patch, $"/api/v1/admin/tours/{Guid.NewGuid()}/restore" };
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/bookings" };
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/payments" };
            yield return new object[] { HttpMethod.Get, "/api/v1/admin/payments/summary" };
        }

        public static IEnumerable<object[]> AdminReadEndpoints()
        {
            yield return new object[] { "/api/v1/admin/dashboard" };
            yield return new object[] { "/api/v1/admin/users" };
            yield return new object[] { $"/api/v1/admin/users/{TestUsers.UserId}" };
            yield return new object[] { "/api/v1/admin/tours" };
            yield return new object[] { "/api/v1/admin/bookings" };
            yield return new object[] { "/api/v1/admin/payments" };
            yield return new object[] { "/api/v1/admin/payments/summary" };
        }

        [Theory]
        [MemberData(nameof(AdminEndpoints))]
        public async Task AdminEndpoints_RejectAnonymousRequests(HttpMethod method, string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateClient();

            var response = await client.SendAsync(new HttpRequestMessage(method, url));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AdminEndpoints))]
        public async Task AdminEndpoints_RejectAuthenticatedNonAdminRoles(HttpMethod method, string url)
        {
            using var factory = CreateFactory();

            using var userClient = factory.CreateAuthenticatedClient(TestUsers.UserRole);
            using var userResponse = await userClient.SendAsync(new HttpRequestMessage(method, url));

            using var guideClient = factory.CreateAuthenticatedClient(TestUsers.GuideRole);
            using var guideResponse = await guideClient.SendAsync(new HttpRequestMessage(method, url));

            Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, guideResponse.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AdminReadEndpoints))]
        public async Task AdminReadEndpoints_AllowAdminRole(string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.AdminRole);

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static AuthorizationTestWebApplicationFactory CreateFactory()
        {
            return new AuthorizationTestWebApplicationFactory(services =>
            {
                services.RemoveAll<IAdminRepository>();
                services.RemoveAll<IBookingRepository>();
                services.RemoveAll<IPaymentRepository>();

                services.AddSingleton(CreateAdminRepository().Object);
                services.AddSingleton(CreateBookingRepository().Object);
                services.AddSingleton(CreatePaymentRepository().Object);
            });
        }

        private static Mock<IAdminRepository> CreateAdminRepository()
        {
            var guide = new TourGuide
            {
                TourGuideId = Guid.NewGuid(),
                FullName = "Admin Test Guide",
                Email = "admin-guide@example.test"
            };

            var repository = new Mock<IAdminRepository>(MockBehavior.Strict);
            repository.Setup(repo => repo.GetTotalUsersAsync()).ReturnsAsync(3);
            repository.Setup(repo => repo.GetTotalGuidesAsync()).ReturnsAsync(1);
            repository.Setup(repo => repo.GetTotalToursAsync()).ReturnsAsync(1);
            repository.Setup(repo => repo.GetTotalBookingsAsync()).ReturnsAsync(1);
            repository.Setup(repo => repo.GetTotalRevenueAsync()).ReturnsAsync(250m);
            repository.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(new[]
            {
                new AdminUserDto
                {
                    Id = TestUsers.UserId,
                    Email = TestUsers.UserEmail,
                    UserName = TestUsers.UserEmail,
                    FullName = "Test Traveller",
                    Roles = new[] { TestUsers.UserRole }
                }
            });
            repository.Setup(repo => repo.GetUserByIdAsync(TestUsers.UserId)).ReturnsAsync(new AdminUserDto
            {
                Id = TestUsers.UserId,
                Email = TestUsers.UserEmail,
                UserName = TestUsers.UserEmail,
                FullName = "Test Traveller",
                Roles = new[] { TestUsers.UserRole }
            });
            repository.Setup(repo => repo.GetAllToursAsync()).ReturnsAsync(new[]
            {
                new Tour
                {
                    TourId = Guid.NewGuid(),
                    Title = "Admin Test Tour",
                    Location = "Banjul",
                    TourGuide = guide,
                    TourGuideId = guide.TourGuideId
                }
            });

            return repository;
        }

        private static Mock<IBookingRepository> CreateBookingRepository()
        {
            var repository = new Mock<IBookingRepository>(MockBehavior.Strict);
            repository
                .Setup(repo => repo.GetAllBookingsAsync(null, null, null, null, true, 1, 10))
                .ReturnsAsync(new List<AdminBookingDto>());

            return repository;
        }

        private static Mock<IPaymentRepository> CreatePaymentRepository()
        {
            var repository = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repository
                .Setup(repo => repo.GetAllPaymentsAsync(null, null, null, null, null, true, 1, 10))
                .ReturnsAsync(new List<Payment>());
            repository
                .Setup(repo => repo.GetPaymentSummaryAsync())
                .ReturnsAsync(new PaymentSummaryDto
                {
                    TotalPayments = 1,
                    SuccessfulPayments = 1,
                    TotalRevenue = 250m
                });

            return repository;
        }
    }
}
