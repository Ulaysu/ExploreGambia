﻿using Asp.Versioning;
using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [ApiVersion("1.0")]  // Specify API version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TourGuideController : ControllerBase
    {
        private readonly ITourGuideRepository tourGuideRepository;
        private readonly IMapper mapper;

        public TourGuideController(ITourGuideRepository tourGuideRepository, IMapper mapper)
        {
            this.tourGuideRepository = tourGuideRepository;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTourGuidesAsync([FromQuery] string? sortBy,
            [FromQuery] bool? isAscending, [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var tourGuideDomainModel = await tourGuideRepository.GetAllAsync(sortBy, isAscending ?? true, searchTerm, pageNumber, pageSize);

            return Ok(mapper.Map<List<TourGuideDto>>(tourGuideDomainModel));
        }

        // Get TourGuide By Id
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<ActionResult<TourGuide>> GetTourGuideById([FromRoute] Guid id)
        {

            var tourGuide = await tourGuideRepository.GetTourGuideByIdAsync(id);

          

            return Ok(mapper.Map<TourGuideDto>(tourGuide));
        }

        // Create Tour
        [HttpPost]
       
        public async Task<IActionResult> CreateTourGuideAsync([FromBody] AddTourGuideRequestDto addTourGuideRequestDto)
        {
            var tourGuideDomainModel = mapper.Map<TourGuide>(addTourGuideRequestDto);

            tourGuideDomainModel = await tourGuideRepository.CreateTourGuideAsync(tourGuideDomainModel);

            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideDomainModel);


            return CreatedAtAction(nameof(GetTourGuideById), new { id = tourGuideDto.TourGuideId }, tourGuideDto);
        }

        // Update TourGuide
        [HttpPut]
        [Route("{id:Guid}")]
        
        public async Task<IActionResult> UpdateTourGuide([FromRoute] Guid id, [FromBody] UpdateTourGuideRequestDto updateTourGuide)
        {

            var tourGuideDomainModel = mapper.Map<TourGuide>(updateTourGuide);

            tourGuideDomainModel = await tourGuideRepository.UpdateTourGuideAsync(id, tourGuideDomainModel);

           

            // Convert Domain Model to DTO
            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideDomainModel);

            return Ok(tourGuideDto);

        }

        // Delete TourGuide
        [HttpDelete]
        [Route("{id:Guid}")]
        // [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteTourGuide([FromRoute] Guid id)
        {
            var tourGuideModel = await tourGuideRepository.DeleteTourGuideAsync(id);

            // Convert Domain Model to DTO
            var tourGuideDto = mapper.Map<TourGuideDto>(tourGuideModel );


            return Ok(tourGuideDto);


        }

    }
}
