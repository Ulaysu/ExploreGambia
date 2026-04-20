using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/tours")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        
        private readonly ITourRepository tourRepository;
        private readonly IMapper mapper;

        public ToursController(ITourRepository tourRepository, IMapper mapper)
        {
            this.tourRepository = tourRepository;
            this.mapper = mapper;
        }

        // Public endpoint - Get all tours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourDto>>> GetAllToursAsync(
           [FromQuery] string? sortBy,
            [FromQuery] bool? isAscending,
            [FromQuery] string? location,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
             [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var tourDomainModel = await tourRepository.GetAllAsync(sortBy, isAscending ?? true,
                location, minPrice, maxPrice, startDate, endDate, pageNumber, pageSize); // Default to ascending if not provided

            return Ok(mapper.Map<List<TourDto>>(tourDomainModel));
        }

        // Public endpoint - Get tour by ID
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Tour>> GetTourById([FromRoute] Guid id)
        {
            var tour = await tourRepository.GetTourById(id);

            return Ok(mapper.Map<TourDto>(tour));
        }

        // Secured endpoint - Create tour (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTour([FromBody] AddTourRequestDto addTourRequestDto)
        {
            // convert from Dto to Domain Model
            var tourDomainModel = mapper.Map<Tour>(addTourRequestDto);

            tourDomainModel = await tourRepository.CreateTourAsync(tourDomainModel);

            // convert from Domain Model to Dto
            var tourDto = mapper.Map<TourDto>(tourDomainModel);

            // return to client
            return CreatedAtAction(nameof(GetTourById), new { id = tourDto.TourId }, tourDto);
        }

        // Secured endpoint - Update tour (Admin only)
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTourGuide([FromRoute] Guid id, [FromBody] UpdateTourRequestDto updateTourRequestDto)
        {

            var tourDomainModel = mapper.Map<Tour>(updateTourRequestDto);

            tourDomainModel = await tourRepository.UpdateTourAsync(id, tourDomainModel);

            // Convert Domain Model to DTO
            var tourDto = mapper.Map<TourDto>(tourDomainModel);

            return Ok(tourDto);

        }

        // Secured endpoint - Delete tour (Admin only)
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTourGuide([FromRoute] Guid id)
        {
            var tourDomainModel = await tourRepository.DeleteTourAsync(id);

            // return deleted Tour back
            // Convert Domain Model to DTO
            var tourDto = mapper.Map<TourDto>(tourDomainModel);

            return Ok(tourDto);

        }

    }
}
