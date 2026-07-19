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
    [Route("api/v{version:apiVersion}/tour-guides")]
    [ApiController]
    public class TourGuideController : ControllerBase
    {
        private readonly ITourGuideRepository tourGuideRepository;
        private readonly ITourGuideService tourGuideService;
        private readonly IMapper mapper;

        public TourGuideController(
            ITourGuideRepository tourGuideRepository,
            ITourGuideService tourGuideService,
            IMapper mapper)
        {
            this.tourGuideRepository = tourGuideRepository;
            this.tourGuideService = tourGuideService;
            this.mapper = mapper;
        }

        // Public endpoint - Get all tour guides
        [HttpGet]
        public async Task<IActionResult> GetAllTourGuidesAsync([FromQuery] string? sortBy,
            [FromQuery] bool? isAscending, [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var tourGuideDomainModel = await tourGuideRepository.GetAllAsync(sortBy, isAscending ?? true, searchTerm, pageNumber, pageSize);

            return Ok(mapper.Map<List<TourGuideDto>>(tourGuideDomainModel));
        }

        // Public endpoint - Get tour guide by ID
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<ActionResult<TourGuide>> GetTourGuideById([FromRoute] Guid id)
        {
            var tourGuide = await tourGuideRepository.GetTourGuideByIdAsync(id);

            return Ok(mapper.Map<TourGuideDto>(tourGuide));
        }

        // Secured endpoint - Create tour guide (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTourGuideAsync([FromBody] AddTourGuideRequestDto addTourGuideRequestDto)
        {
            var tourGuideDomainModel = mapper.Map<TourGuide>(addTourGuideRequestDto);

            tourGuideDomainModel = await tourGuideRepository.CreateTourGuideAsync(tourGuideDomainModel);

            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideDomainModel);

            return CreatedAtAction(nameof(GetTourGuideById), new { id = tourGuideDto.TourGuideId }, tourGuideDto);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Guide")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var guide = await tourGuideRepository.GetTourGuideByUserIdAsync(userId);

            if (guide == null)
                return NotFound("Tour guide profile not found for this user");

            var dto = mapper.Map<TourGuideProfileDto>(guide);

            return Ok(dto);
        }

        // Secured endpoint - Update tour guide (Admin only)
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTourGuide([FromRoute] Guid id, [FromBody] UpdateTourGuideRequestDto updateTourGuide)
        {
            var tourGuideDomainModel = mapper.Map<TourGuide>(updateTourGuide);

            tourGuideDomainModel = await tourGuideRepository.UpdateTourGuideAsync(id, tourGuideDomainModel);

            // Convert Domain Model to DTO
            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideDomainModel);

            return Ok(tourGuideDto);
        }

        // Secured endpoint - Delete tour guide (Admin only)
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTourGuide([FromRoute] Guid id)
        {
            var tourGuideModel = await tourGuideService.DeleteTourGuideAsync(id);

            // Convert Domain Model to DTO
            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideModel);

            return Ok(tourGuideDto);
        }


        [HttpPut("me")]
        [Authorize(Roles = "Guide")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTourGuideProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var guide = await tourGuideRepository.GetTourGuideByUserIdAsync(userId);

            if (guide == null)
                return NotFound("Tour guide profile not found");

            // update fields
            guide.PhoneNumber = dto.PhoneNumber;
            guide.Bio = dto.Bio;
            guide.IsAvailable = dto.IsAvailable;

            await tourGuideRepository.UpdateTourGuideProfileAsync(guide);

            var result = mapper.Map<TourGuideProfileDto>(guide);

            return Ok(result);
        }
    }
}
