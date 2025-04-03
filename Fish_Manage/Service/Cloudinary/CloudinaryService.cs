using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudinaryAccount = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(cloudinaryAccount);
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream())
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }
}
