using Asp.Versioning;
using ExploreGambia.API.Models.Configurations;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly UserManager<ApplicationUser> userManager;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            this.authService = authService;
            this.userManager = userManager;
        }

        /// <summary>
        /// Register a new user with email, password, and optional roles
        /// </summary>
        [EnableRateLimiting(AuthRateLimitPolicyNames.Register)]
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (!ModelState.IsValid)      {
                return BadRequest(ModelState);
            }

            var response = await authService.RegisterAsync(registerRequestDto);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [EnableRateLimiting(AuthRateLimitPolicyNames.Login)]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto loginRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await authService.LoginAsync(loginRequestDto);

            if (string.IsNullOrEmpty(response.JwtToken))
            {
                return Unauthorized(new { message = response.Error ?? "Login failed" });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get authenticated user profile with roles and personal info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(AuthMeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                return Unauthorized(new { message = "Email not found in token claims" });
            }

            var user = await userManager.FindByEmailAsync(emailClaim);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var roles = await userManager.GetRolesAsync(user);

            var response = new AuthMeResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                IsAuthenticated = true
            };

            return Ok(response);
        }

        /// <summary>
        /// Update authenticated user's basic profile information
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCurrentUserAsync([FromBody] UpdateAuthMeRequestDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                return Unauthorized(new { message = "Email not found in token claims" });
            }

            var user = await userManager.FindByEmailAsync(emailClaim);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        /// <summary>
        /// Logout the authenticated user and revoke their active refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync()
        {
            // Use the JWT subject identifier so logout cannot target another user's refresh token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "User ID not found in token claims" });
            }

            await authService.LogoutAsync(userId);

            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Rotate a valid refresh token and issue a new access token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokenAsync(request);
            return Ok(result);
        }
    }
}
