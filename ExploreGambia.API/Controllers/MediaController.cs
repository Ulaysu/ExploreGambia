using Asp.Versioning;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExploreGambia.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/media")]
    [ApiVersion("1.0")]
    public class MediaController : ControllerBase
    {

        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var imageUrl = await _mediaService.UploadAsync(file);

            return Ok(new
            {
                imageUrl
            });
        }

    }
}
