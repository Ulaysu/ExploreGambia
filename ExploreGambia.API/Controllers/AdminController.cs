using AutoMapper;
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
        private readonly IMapper _mapper;

        public AdminController(IAdminRepository adminRepo, IMapper mapper)
        {
            _adminRepo = adminRepo;
            _mapper = mapper;
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

        [HttpGet("tours")]
        public async Task<IActionResult> GetAllTours()
        {
            var tours = await _adminRepo.GetAllToursAsync();

            return Ok(_mapper.Map<List<AdminTourDto>>(tours));
        }

        [HttpPatch("tours/{id:guid}/delete")]
        public async Task<IActionResult> SoftDeleteTour(Guid id)
        {
            var deleted =
                await _adminRepo.SoftDeleteTourAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Tour Deleted Successfully"});
        }

        [HttpPatch("tours/{id:guid}/restore")]
        public async Task<IActionResult> RestoreTour(Guid id)
        {
            var restored =
                await _adminRepo.RestoreTourAsync(id);

            if (!restored)
                return NotFound();

            return Ok(new {Message = "Tour Restored Successfully"});
        }
    }
}
