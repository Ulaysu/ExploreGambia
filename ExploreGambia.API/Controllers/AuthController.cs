using Asp.Versioning;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                IsAuthenticated = true
            };

            return Ok(response);
        }
    }
}
