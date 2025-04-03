using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/[controller]")]
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

        // Get All Tours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetAllToursAsync()
        {
            var tourDomainModel = await tourRepository.GetAllAsync();

            return Ok(mapper.Map<List<TourDto>>(tourDomainModel));
        }

        // Get Tour By Id
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Tour>> GetTourById([FromRoute] Guid id)
        {

            var tour = await tourRepository.GetTourById(id);

            if(tour == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<TourDto>(tour));
        }

        // Create Tour
        [HttpPost]
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

        // Update Tour
        [HttpPut]
        [Route("{id:Guid}")]

        public async Task<IActionResult> UpdateTourGuide([FromRoute] Guid id, [FromBody] UpdateTourRequestDto updateTourRequestDto)
        {

            var tourDomainModel = mapper.Map<Tour>(updateTourRequestDto);

            tourDomainModel = await tourRepository.UpdateTourAsync(id, tourDomainModel);

            if (tourDomainModel == null) return NotFound();

            // Convert Domain Model to DTO
            var tourDto = mapper.Map<TourDto>(tourDomainModel);

            return Ok(tourDto);

        }

        // Delete Tour
        [HttpDelete]
        [Route("{id:Guid}")]
        // [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteTourGuide([FromRoute] Guid id)
        {
            var tourDomainModel = await tourRepository.DeleteTourAsync(id);

            if (tourDomainModel == null)
            {
                return NotFound();
            }

            // return deleted Tour back
            // Convert Domain Model to DTO
            var tourDto = mapper.Map<TourDto>(tourDomainModel);


            return Ok(tourDto);


        }


    }
}
