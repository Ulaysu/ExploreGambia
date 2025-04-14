using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // [Authorize]
   
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository bookingRepository;
        private readonly ITourRepository tourRepository;
        private readonly IMapper mapper;

        public BookingController(IBookingRepository bookingRepository, ITourRepository tourRepository, IMapper mapper)
        {
            this.bookingRepository = bookingRepository;
            this.tourRepository = tourRepository;
            this.mapper = mapper;
        }

        //[Authorize(Roles = "User, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await bookingRepository.GetAllBookingsAsync();

            return Ok(mapper.Map<List<BookingDto>>(bookings));


        }

        // Get Tour By Id
        //[Authorize(Roles = "User, Admin")]
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Booking>> GetBookingById([FromRoute] Guid id)
        {

            var booking = await bookingRepository.GetBookingById(id);

            
            return Ok(mapper.Map<BookingDto>(booking));
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] AddBookingRequestDto addBookingRequestDto)
        {
            // Validating the request
            if (addBookingRequestDto == null || addBookingRequestDto.NumberOfPeople <= 0)
            {
                return BadRequest("Invalid booking request.");
            }

            // Checking if the tour exists
            var tour = await tourRepository.GetTourById(addBookingRequestDto.TourId);
            if (tour == null)
            {
                return NotFound("Tour not found.");
            }

            // Checking tour availability
            if (addBookingRequestDto.NumberOfPeople > tour.MaxParticipants)
            {
                return BadRequest("The number of people exceeds the tour's maximum allowed participants.");
            }

            // Convert DTO to Domain Model using AutoMapper
            var booking = mapper.Map<Booking>(addBookingRequestDto);
            booking.BookingId = Guid.NewGuid(); // Assign a new BookingId
            booking.BookingDate = DateTime.UtcNow;
            booking.TotalAmount = tour.Price * addBookingRequestDto.NumberOfPeople; // Calculate cost
            booking.Status = BookingStatus.Pending;

            // Save to database
            booking = await bookingRepository.CreateBookingAsync(booking);

            // Convert to Response DTO using AutoMapper
            var bookingDto = mapper.Map<BookingDto>(booking);

            // Return Created response with the new booking
            return CreatedAtAction(nameof(GetBookingById), new { id = bookingDto.BookingId }, bookingDto);
        }

         [HttpPut]
         [Route("{id:guid}")]
         public async Task<IActionResult> UpdateBooking([FromRoute] Guid id, [FromBody] UpdateBookingRequestDto updateBookingRequestDto)
         {
             var booking = mapper.Map<Booking>(updateBookingRequestDto);

             booking = await bookingRepository.UpdateBookingAsync(id, booking);
             if (booking == null)
             {
                return NotFound("Could not update booking. Check if the Booking ID is correct and if the specified Tour ID exists.");
            }

             return Ok(mapper.Map<BookingDto>(booking)); 
         }

       
        // Delete Booking
        //[Authorize(Roles = "Admin")]
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
