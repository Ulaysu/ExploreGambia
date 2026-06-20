using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/bookings")]
    [ApiController]
    [Authorize]
   
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository bookingRepository;
        private readonly IBookingService bookingService;
        private readonly IMapper mapper;

        public BookingController(IBookingRepository bookingRepository, IBookingService bookingService, IMapper mapper)
        {
            this.bookingRepository = bookingRepository;
            this.bookingService = bookingService;
            this.mapper = mapper;
        }

        [Authorize(Roles = "User, Admin")]
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User identity could not be determined.");

            var bookings = await bookingService.GetMyBookingsAsync(userId);

            return Ok(mapper.Map<List<BookingDto>>(bookings));
        }

        [Authorize(Roles = "User, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] BookingStatus? status,
    [FromQuery] DateTime? bookingDateFrom, [FromQuery] DateTime? bookingDateTo, [FromQuery] string? sortBy,
            [FromQuery] bool? isAscending, [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var bookings = await bookingRepository.GetAllBookingsAsync(status, bookingDateFrom, bookingDateTo, sortBy, isAscending ?? true, pageNumber, pageSize);

            return Ok(mapper.Map<List<BookingDto>>(bookings));


        }

        
        [Authorize(Roles = "User, Admin")]
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Booking>> GetBookingById([FromRoute] Guid id)
        {

            var booking = await bookingRepository.GetBookingById(id);

            
            return Ok(mapper.Map<BookingDto>(booking));
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] AddBookingRequestDto addBookingRequestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User identity could not be determined.");

            var booking = await bookingService.CreateBookingAsync(addBookingRequestDto, userId);

            // Convert to Response DTO using AutoMapper
            var bookingDto = mapper.Map<BookingDto>(booking);

            // Return Created response with the new booking
            return CreatedAtAction(nameof(GetBookingById), new { id = bookingDto.BookingId }, bookingDto);
        }

         [HttpPut]
         [Route("{id:guid}")]
        [Authorize(Roles = "User")]
         public async Task<IActionResult> UpdateBooking([FromRoute] Guid id, [FromBody] UpdateBookingRequestDto updateBookingRequestDto)
         {
             var booking = await bookingService.UpdateBookingAsync(id, updateBookingRequestDto);
             if (booking == null)
             {
                return NotFound("Could not update booking. Check if the Booking ID is correct and if the specified Tour ID exists.");
            }

             return Ok(mapper.Map<BookingDto>(booking)); 
         }

       
        // Delete Booking
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id:Guid}")]
        // 
        public async Task<IActionResult> DeleteBooking([FromRoute] Guid id)
        {
            var booking = await bookingRepository.DeleteBookingAsync(id);

            if (booking == null)
            {
                return NotFound();
            }


            return Ok(new { Message = $"Booking with ID '{id}' deleted successfully." });


        }

    }
}
