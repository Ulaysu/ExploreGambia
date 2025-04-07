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
    //[Authorize]
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

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await bookingRepository.GetAllBookingsAsync();

            return Ok(mapper.Map<List<BookingDto>>(bookings));


        }

        // Get Tour By Id
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Booking>> GetBookingById([FromRoute] Guid id)
        {

            var booking = await bookingRepository.GetBookingById(id);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<BookingDto>(booking));
        }

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
                 return NotFound("Booking not found.");
             }

             return Ok(mapper.Map<BookingDto>(booking)); 
         }

       /* [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBooking([FromRoute] Guid id, UpdateBookingRequestDto updateBookingRequestDto)
        {
            // 1. Retrieve the existing booking from the database using the ID from the route.
            var existingBooking = await bookingRepository.GetBookingById(id);

            if (existingBooking == null)
            {
                return NotFound("Booking not found.");
            }

            // 2. Update the properties of the existing booking with values from the DTO.
            existingBooking.NumberOfPeople = updateBookingRequestDto.NumberOfPeople;
            existingBooking.Status = updateBookingRequestDto.Status;

            // 3. (Optional) If you want to allow updating the BookingDate:
            // existingBooking.BookingDate = updateBookingRequestDto.BookingDate; // Add this to your DTO if needed

            // 4. Fetch the associated Tour to recalculate the TotalAmount.
            var tour = await tourRepository.GetTourById(existingBooking.TourId); // Assuming you have a Tour repository

            if (tour == null)
            {
                return BadRequest("Associated Tour not found."); // Handle the case where the tour might be deleted
            }

            existingBooking.TotalAmount = existingBooking.NumberOfPeople * tour.Price;

            // 5. Call the repository to update the *existing* booking.
            var updatedBooking = await bookingRepository.UpdateBookingAsync(existingBooking);

            if (updatedBooking == null) // This check might not be necessary now as you fetched it first
            {
                return NotFound("Booking not found.");
            }

            return Ok(mapper.Map<BookingDto>(updatedBooking));
        }*/

        // Delete Booking
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

            // return deleted booking back
            // Convert Domain Model to DTO
            var tourDto = mapper.Map<BookingDto>(booking);


            return Ok(tourDto);


        }

    }
}
