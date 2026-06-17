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
    }
}
