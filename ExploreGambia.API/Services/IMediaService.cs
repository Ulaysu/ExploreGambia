namespace ExploreGambia.API.Services
{
    public interface IMediaService
    {
        Task<string> UploadAsync(IFormFile file);
    }
}
