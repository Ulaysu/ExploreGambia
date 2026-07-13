using System.Net;
using System.Net.Http.Json;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ExploreGambia.API.Tests.Authorization
{
    public class SharedAuthorizationIntegrationTests
    {
        private static readonly Guid TourId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        private static readonly Guid BookingId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        public static IEnumerable<object[]> SharedProtectedEndpoints()
        {
            yield return new object[] { HttpMethod.Get, "/api/v1/auth/me" };
            yield return new object[] { HttpMethod.Put, "/api/v1/auth/me" };
            yield return new object[] { HttpMethod.Post, "/api/v1/auth/logout" };
            yield return new object[] { HttpMethod.Get, "/api/v1/bookings/my-bookings" };
            yield return new object[] { HttpMethod.Post, "/api/v1/bookings" };
        }

        public static IEnumerable<object[]> AuthenticatedRoles()
        {
            yield return new object[] { TestUsers.UserRole };
            yield return new object[] { TestUsers.GuideRole };
            yield return new object[] { TestUsers.AdminRole };
        }

        [Theory]
        [MemberData(nameof(SharedProtectedEndpoints))]
        public async Task SharedProtectedEndpoints_RejectAnonymousRequests(HttpMethod method, string url)
        {
            using var factory = CreateFactory();
            var client = factory.CreateClient();

            var response = await client.SendAsync(CreateSharedRequest(method, url));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AuthenticatedRoles))]
        public async Task AuthMe_AllAuthenticatedRolesCanReadProfile(string role)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(role);

            var response = await client.GetAsync("/api/v1/auth/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AuthenticatedRoles))]
        public async Task AuthMe_AllAuthenticatedRolesCanUpdateProfile(string role)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(role);

            var response = await client.PutAsJsonAsync(
                "/api/v1/auth/me",
                new UpdateAuthMeRequestDto
                {
                    FirstName = "Updated",
                    LastName = "User"
                });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AuthenticatedRoles))]
        public async Task Logout_AllAuthenticatedRolesCanLogout(string role)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(role);

            var response = await client.PostAsync("/api/v1/auth/logout", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, factory.AuthService.LogoutCalls);
            Assert.Equal(TestUsers.UserIdFor(role), factory.AuthService.LastLoggedOutUserId);
        }

        [Theory]
        [InlineData(TestUsers.UserRole)]
        [InlineData(TestUsers.AdminRole)]
        public async Task MyBookings_AllowsUserAndAdminRoles(string role)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(role);

            var response = await client.GetAsync("/api/v1/bookings/my-bookings");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task MyBookings_RejectsGuideRole()
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.GuideRole);

            var response = await client.GetAsync("/api/v1/bookings/my-bookings");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateBooking_AllowsUserRole()
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(TestUsers.UserRole);

            var response = await client.PostAsJsonAsync("/api/v1/bookings", CreateBookingRequest());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUsers.GuideRole)]
        [InlineData(TestUsers.AdminRole)]
        public async Task CreateBooking_RejectsNonUserRoles(string role)
        {
            using var factory = CreateFactory();
            var client = factory.CreateAuthenticatedClient(role);

            var response = await client.PostAsJsonAsync("/api/v1/bookings", CreateBookingRequest());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static AuthorizationTestWebApplicationFactory CreateFactory()
        {
            return new AuthorizationTestWebApplicationFactory(services =>
            {
                services.RemoveAll<UserManager<ApplicationUser>>();
                services.RemoveAll<IBookingService>();
                services.RemoveAll<IBookingRepository>();

                services.AddSingleton(CreateUserManager().Object);
                services.AddSingleton(CreateBookingService().Object);
                services.AddSingleton(new Mock<IBookingRepository>(MockBehavior.Loose).Object);
            });
        }

        private static HttpRequestMessage CreateSharedRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Put)
            {
                request.Content = JsonContent.Create(new UpdateAuthMeRequestDto
                {
                    FirstName = "Updated",
                    LastName = "User"
                });
            }
            else if (method == HttpMethod.Post && url.Contains("/bookings", StringComparison.OrdinalIgnoreCase))
            {
                request.Content = JsonContent.Create(CreateBookingRequest());
            }

            return request;
        }

        private static AddBookingRequestDto CreateBookingRequest()
        {
            return new AddBookingRequestDto
            {
                TourId = TourId,
                NumberOfPeople = 2
            };
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            userManager
                .Setup(manager => manager.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => new ApplicationUser
                {
                    Id = UserIdForEmail(email),
                    Email = email,
                    UserName = email,
                    FirstName = "Authorization",
                    LastName = "User",
                    IsActive = true
                });
            userManager
                .Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser user) => new[] { RoleForEmail(user.Email ?? string.Empty) });
            userManager
                .Setup(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            return userManager;
        }

        private static Mock<IBookingService> CreateBookingService()
        {
            var bookingService = new Mock<IBookingService>(MockBehavior.Strict);
            bookingService
                .Setup(service => service.GetMyBookingsAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Booking> { CreateBooking(TestUsers.UserId) });
            bookingService
                .Setup(service => service.CreateBookingAsync(
                    It.IsAny<AddBookingRequestDto>(),
                    TestUsers.UserId))
                .ReturnsAsync((AddBookingRequestDto request, string userId) => CreateBooking(userId, request.NumberOfPeople));

            return bookingService;
        }

        private static Booking CreateBooking(string userId, int numberOfPeople = 2)
        {
            var guide = new TourGuide
            {
                TourGuideId = Guid.NewGuid(),
                UserId = TestUsers.GuideId,
                FullName = "Booking Test Guide",
                Email = TestUsers.GuideEmail
            };

            return new Booking
            {
                BookingId = BookingId,
                TourId = TourId,
                UserId = userId,
                NumberOfPeople = numberOfPeople,
                TotalAmount = 50m,
                Tour = new Tour
                {
                    TourId = TourId,
                    TourGuideId = guide.TourGuideId,
                    TourGuide = guide,
                    Title = "Booking Test Tour",
                    Description = "A booking tour used by authorization tests.",
                    Location = "Banjul",
                    Price = 25m,
                    MaxParticipants = 8,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2)
                }
            };
        }

        private static string UserIdForEmail(string email)
        {
            return email switch
            {
                TestUsers.UserEmail => TestUsers.UserId,
                TestUsers.GuideEmail => TestUsers.GuideId,
                TestUsers.AdminEmail => TestUsers.AdminId,
                _ => "authorization-unknown-id"
            };
        }

        private static string RoleForEmail(string email)
        {
            return email switch
            {
                TestUsers.UserEmail => TestUsers.UserRole,
                TestUsers.GuideEmail => TestUsers.GuideRole,
                TestUsers.AdminEmail => TestUsers.AdminRole,
                _ => TestUsers.UserRole
            };
        }
    }
}
