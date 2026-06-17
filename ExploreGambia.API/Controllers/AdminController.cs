using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepo;

        public AdminController(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo;
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = new AdminDashboardDto
            {
                TotalUsers = await _adminRepo.GetTotalUsersAsync(),
                TotalGuides = await _adminRepo.GetTotalGuidesAsync(),
                TotalTours = await _adminRepo.GetTotalToursAsync(),
                TotalBookings = await _adminRepo.GetTotalBookingsAsync(),
                Revenue = await _adminRepo.GetTotalRevenueAsync()
            };

            return Ok(result);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminRepo.GetAllUsersAsync();

            return Ok(users);
        }


        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser( [FromRoute] string id)
        {
            var user = await _adminRepo.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus([FromRoute] string id,
            [FromBody] UpdateUserStatusRequestDto request)
        {
            var updated =
                await _adminRepo.UpdateUserStatusAsync( id, request.IsActive);

            if (!updated)
                return NotFound();

            return Ok();
        }
    }
}
