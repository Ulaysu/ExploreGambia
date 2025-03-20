using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttractionsController : ControllerBase
    {
        private readonly ExploreGambiaDbContext _context;
        public AttractionsController(ExploreGambiaDbContext context)
        {
            _context = context;
        }

        // Get all Attractions 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attraction>>> GetAllAttractions()
        {
            return await _context.Attractions.ToListAsync();
        }

        // Get an Attraction by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Attraction>> GetAttraction(Guid id)
        {
            var attraction = await _context.Attractions.FindAsync(id);
            if (attraction == null) return NotFound();
            return Ok(attraction);
        }


    }
}
