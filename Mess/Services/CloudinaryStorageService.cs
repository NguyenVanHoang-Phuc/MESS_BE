using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MESS.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly LocalFileStorageService _fallbackStorage;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(
        IConfiguration configuration,
        LocalFileStorageService fallbackStorage,
        ILogger<CloudinaryStorageService> logger)
    {
        _fallbackStorage = fallbackStorage;
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"] ?? configuration["Cloudinary__CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"] ?? configuration["Cloudinary__ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"] ?? configuration["Cloudinary__ApiSecret"];

        if (!string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            try
            {
                var account = new Account(cloudName.Trim(), apiKey.Trim(), apiSecret.Trim());
                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
                _logger.LogInformation("Cloudinary initialized successfully for Cloud: {CloudName}", cloudName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Cloudinary. Falling back to Local Storage.");
                _cloudinary = null;
            }
        }
        else
        {
            _logger.LogWarning("Cloudinary credentials not provided in .env. Falling back to Local Storage.");
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        if (_cloudinary != null)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var contentType = file.ContentType?.ToLower() ?? "";
                var isImage = contentType.StartsWith("image/");
                var isVideo = contentType.StartsWith("video/");

                if (isImage)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"mess_chat/{folder}",
                        PublicId = $"{Guid.NewGuid():N}",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        _logger.LogInformation("Uploaded image {FileName} to Cloudinary: {Url}", file.FileName, url);
                        return url;
                    }
                }
                else if (isVideo)
                {
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"mess_chat/{folder}",
                        PublicId = $"{Guid.NewGuid():N}",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        _logger.LogInformation("Uploaded video {FileName} to Cloudinary: {Url}", file.FileName, url);
                        return url;
                    }
                }
                else
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"mess_chat/{folder}",
                        PublicId = $"{Guid.NewGuid():N}_{file.FileName}",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        _logger.LogInformation("Uploaded document {FileName} to Cloudinary: {Url}", file.FileName, url);
                        return url;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Cloudinary. Falling back to Local Storage.");
            }
        }

        // Fallback to local storage
        return await _fallbackStorage.SaveFileAsync(file, folder);
    }
}
