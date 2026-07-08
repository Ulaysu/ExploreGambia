
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Security.Claims;

namespace ExploreGambia.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly ITourGuideRepository tourGuideRepository;

        public AuthService(UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository,
            ITourGuideRepository tourGuideRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.tourGuideRepository = tourGuideRepository;

        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            // ✅ Validate Email and Password only
            if (string.IsNullOrWhiteSpace(registerRequestDto.Email) ||
                string.IsNullOrWhiteSpace(registerRequestDto.Password))
            {
                return new RegisterResponseDto
                {
                    IsSuccess = false,
                    Message = "Email and password are required.",
                    Errors = new List<string> { "Missing required fields" }
                };
            }

            // ✅ Email as UserName internally
            var applicationUser = new ApplicationUser
            {
                UserName = registerRequestDto.Email,
                Email = registerRequestDto.Email,
                FirstName = registerRequestDto.FirstName ?? string.Empty,
                LastName = registerRequestDto.LastName ?? string.Empty,
                IsActive = true
            };

            var identityResult = await userManager.CreateAsync(applicationUser, registerRequestDto.Password);

            if (identityResult.Succeeded)
            {
                // Add roles if provided
                if (registerRequestDto.Roles != null && registerRequestDto.Roles.Any())
                {
                    identityResult = await userManager.AddToRolesAsync(applicationUser, registerRequestDto.Roles);

                    if (identityResult.Succeeded)
                    {
                        var roles = registerRequestDto.Roles.Select(r => r.ToLower()).ToList();

                        if (roles.Contains("guide"))
                        {
                            var existingGuide = await tourGuideRepository
                                .GetTourGuideByUserIdAsync(applicationUser.Id);

                            if (existingGuide == null)
                            {
                                var tourGuide = new TourGuide
                                {
                                    TourGuideId = Guid.NewGuid(),
                                    UserId = applicationUser.Id,
                                    FullName = $"{applicationUser.FirstName} {applicationUser.LastName}",
                                    Email = applicationUser.Email!,
                                    PhoneNumber = string.Empty,
                                    Bio = string.Empty,
                                    IsAvailable = true
                                };

                                await tourGuideRepository.CreateTourGuideAsync(tourGuide);
                            }
                        }
                        return new RegisterResponseDto
                        {
                            IsSuccess = true,
                            Message = "User was registered successfully! Please login."
                        };
                    }
                    else
                    {
                        // ✅ Log using Email
                        Log.Error("Failed to add roles '{Roles}' to user '{Email}'. Errors: {@Errors}",
                                  string.Join(",", registerRequestDto.Roles),
                                  registerRequestDto.Email,
                                  identityResult.Errors);

                        return new RegisterResponseDto
                        {
                            IsSuccess = false,
                            Message = "Failed to assign roles during registration.",
                            Errors = identityResult.Errors.Select(e => e.Description).ToList()
                        };
                    }
                }

                return new RegisterResponseDto
                {
                    IsSuccess = true,
                    Message = "User was registered successfully! Please login."
                };
            }

            // ✅ Log using Email
            Log.Error("User registration failed for '{Email}'. Errors: {@Errors}",
                      registerRequestDto.Email,
                      identityResult.Errors);

            return new RegisterResponseDto
            {
                IsSuccess = false,
                Message = "Registration failed.",
                Errors = identityResult.Errors.Select(e => e.Description).ToList()
            };
        }

        /*public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await userManager.FindByEmailAsync(loginRequestDto.Email);

            if (user != null)
            {
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (checkPasswordResult)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());

                    // THIS is where GenerateRefreshToken gets called
                    var refreshToken = tokenRepository.GenerateRefreshToken();

                    // Stored in the database against the user
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
                    await userManager.UpdateAsync(user);

                    return new LoginResponseDto { 
                        JwtToken = jwtToken,
                        RefreshToken = refreshToken};
                }
            }

            return new LoginResponseDto { JwtToken = string.Empty, Error = "Email or password incorrect" };
        }*/

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await userManager.FindByEmailAsync(loginRequestDto.Email);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    JwtToken = string.Empty,
                    Error = "Email or password incorrect"
                };
            }

            var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (!checkPasswordResult)
            {
                return new LoginResponseDto
                {
                    JwtToken = string.Empty,
                    Error = "Email or password incorrect"
                };
            }

            // User Guard
            if (!user.IsActive)
            {
                return new LoginResponseDto
                {
                    JwtToken = string.Empty,
                    Error = "Account is disabled. Contact admin."
                };
            }

            var roles = await userManager.GetRolesAsync(user);
            var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());

            var refreshToken = tokenRepository.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            await userManager.UpdateAsync(user);

            return new LoginResponseDto
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
                throw new AuthenticationFailedException("Access token is required.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new AuthenticationFailedException("Refresh token is required.");

            var principal = tokenRepository.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await userManager.FindByEmailAsync(email!)
                ?? throw new AuthenticationFailedException("Invalid token.");

            // A cleared token or expiry means the stored refresh token has been revoked, such as after logout.
            if (string.IsNullOrWhiteSpace(user.RefreshToken))
                throw new AuthenticationFailedException("Invalid refresh token.");

            if (user.RefreshTokenExpiryTime == null)
                throw new AuthenticationFailedException("Invalid refresh token.");

            if (user.RefreshToken != request.RefreshToken)
                throw new AuthenticationFailedException("Invalid refresh token.");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new AuthenticationFailedException("Refresh token has expired. Please log in again.");

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = tokenRepository.CreateJWTToken(user, roles.ToList());
            var newRefreshToken = tokenRepository.GenerateRefreshToken();

            // ✅ Rotate the refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await userManager.UpdateAsync(user);

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

        }

        public async Task LogoutAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId)
                ?? throw new AuthenticationFailedException("User not found.");

            // The current auth model stores one active refresh token per user, so clearing both fields revokes it.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await userManager.UpdateAsync(user);
        }
    }
}
