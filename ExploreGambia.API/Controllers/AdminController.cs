using AutoMapper;
using ExploreGambia.API.Models.Domain;
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
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;

        public AdminController(IAdminRepository adminRepo, IMapper mapper, 
            IBookingRepository bookingRepository, IPaymentRepository paymentRepository)
        {
            _adminRepo = adminRepo;
            _mapper = mapper;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTours()
        {
            var tours = await _adminRepo.GetAllToursAsync();

            return Ok(_mapper.Map<List<AdminTourDto>>(tours));
        }

        [HttpPatch("tours/{id:guid}/delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteTour(Guid id)
        {
            var deleted =
                await _adminRepo.SoftDeleteTourAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Tour Deleted Successfully"});
        }

        [HttpPatch("tours/{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreTour(Guid id)
        {
            var restored =
                await _adminRepo.RestoreTourAsync(id);

            if (!restored)
                return NotFound();

            return Ok(new {Message = "Tour Restored Successfully"});
        }

        [HttpGet("bookings")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings(
    [FromQuery] BookingStatus? status,
    [FromQuery] DateTime? bookingDateFrom,
    [FromQuery] DateTime? bookingDateTo,
    [FromQuery] string? sortBy,
    [FromQuery] bool? isAscending,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync(
                status,
                bookingDateFrom,
                bookingDateTo,
                sortBy,
                isAscending ?? true,
                pageNumber,
                pageSize);

            return Ok(_mapper.Map<List<AdminBookingDto>>(bookings));
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments(
    [FromQuery] string? paymentMethod,
    [FromQuery] DateTime? paymentDateFrom,
    [FromQuery] DateTime? paymentDateTo,
    [FromQuery] PaymentStatus? status,
    [FromQuery] string? sortBy,
    [FromQuery] bool? isAscending,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var payments =
                await _paymentRepository.GetAllPaymentsAsync(
                    paymentMethod,
                    paymentDateFrom,
                    paymentDateTo,
                    status,
                    sortBy,
                    isAscending ?? true,
                    pageNumber,
                    pageSize);

            return Ok(_mapper.Map<List<PaymentDto>>(payments));
        }

        [HttpGet("payments/summary")]
        public async Task<IActionResult> GetPaymentSummary()
        {
            var summary = await _paymentRepository.GetPaymentSummaryAsync();

            return Ok(summary);
        }
    }
}
