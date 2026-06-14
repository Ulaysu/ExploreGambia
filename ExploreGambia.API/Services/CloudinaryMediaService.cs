using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ExploreGambia.API.Services
{
    public class CloudinaryMediaService : IMediaService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryMediaService(IConfiguration configuration)
        {
            var cloudName = configuration["CLOUDINARY_CLOUD_NAME"];
            var apiKey = configuration["CLOUDINARY_API_KEY"];
            var apiSecret = configuration["CLOUDINARY_API_SECRET"];

            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary environment variables are not configured.");
            }

            var account = new Account(
                cloudName,
                apiKey,
                apiSecret);

            _cloudinary = new Cloudinary(account);
        }
        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded.");
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "explore-gambia"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception(result.Error.Message);
            }

            return result.SecureUrl.ToString();
        }
    }
}
