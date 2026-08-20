using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MESS.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string _azureContainerName;
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

        // 1. Container Name configuration (mặc định 'mess' hoặc từ .env)
        _azureContainerName = configuration["BlobSettings:ContainerName"]
                           ?? configuration["BlobSettings__ContainerName"]
                           ?? configuration["Azure:ContainerName"]
                           ?? configuration["Azure__ContainerName"]
                           ?? Environment.GetEnvironmentVariable("BlobSettings__ContainerName")
                           ?? Environment.GetEnvironmentVariable("Azure__ContainerName")
                           ?? "mess";

        // 2. Check Azure Blob Storage Connection String
        var azureConnStr = configuration["Azure:BlobConnectionString"] 
                        ?? configuration["Azure__BlobConnectionString"]
                        ?? configuration["BlobSettings:ConnectionString"]
                        ?? configuration["BlobSettings__ConnectionString"]
                        ?? Environment.GetEnvironmentVariable("Azure__BlobConnectionString")
                        ?? Environment.GetEnvironmentVariable("BlobSettings__ConnectionString")
                        ?? Environment.GetEnvironmentVariable("Azure:BlobConnectionString");

        if (!string.IsNullOrWhiteSpace(azureConnStr))
        {
            try
            {
                _blobServiceClient = new BlobServiceClient(azureConnStr.Trim());
                _logger.LogInformation("Azure Blob Storage initialized successfully with container: {ContainerName}", _azureContainerName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Azure Blob Storage. Will fallback to Cloudinary / Local.");
                _blobServiceClient = null;
            }
        }
        else
        {
            _logger.LogInformation("Azure Blob Storage connection string not configured.");
        }

        // 2. Check Cloudinary Credentials
        var cloudName = configuration["Cloudinary:CloudName"] 
                     ?? configuration["Cloudinary__CloudName"]
                     ?? Environment.GetEnvironmentVariable("Cloudinary__CloudName")
                     ?? Environment.GetEnvironmentVariable("Cloudinary:CloudName");

        var apiKey = configuration["Cloudinary:ApiKey"] 
                  ?? configuration["Cloudinary__ApiKey"]
                  ?? Environment.GetEnvironmentVariable("Cloudinary__ApiKey")
                  ?? Environment.GetEnvironmentVariable("Cloudinary:ApiKey");

        var apiSecret = configuration["Cloudinary:ApiSecret"] 
                     ?? configuration["Cloudinary__ApiSecret"]
                     ?? Environment.GetEnvironmentVariable("Cloudinary__ApiSecret")
                     ?? Environment.GetEnvironmentVariable("Cloudinary:ApiSecret");

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
                _logger.LogWarning(ex, "Failed to initialize Cloudinary. Will fallback to Local Storage.");
                _cloudinary = null;
            }
        }
        else
        {
            _logger.LogInformation("Cloudinary credentials not provided in .env.");
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        // 1. Try Azure Blob Storage first if configured
        if (_blobServiceClient != null)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_azureContainerName);

                // Create container if not existing (safe for both public/private accounts)
                try
                {
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                }
                catch
                {
                    try
                    {
                        await containerClient.CreateIfNotExistsAsync();
                    }
                    catch
                    {
                        // Ignored if already exists or permission restrictions
                    }
                }

                var ext = Path.GetExtension(file.FileName);
                var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
                var blobName = $"mess_chat/{folder}/{Guid.NewGuid():N}_{safeFileName}{ext}";

                var blobClient = containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    var uploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = string.IsNullOrWhiteSpace(file.ContentType) 
                                ? "application/octet-stream" 
                                : file.ContentType
                        }
                    };
                    await blobClient.UploadAsync(stream, uploadOptions);
                }

                string azureUrl;
                if (blobClient.CanGenerateSasUri)
                {
                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = _azureContainerName,
                        BlobName = blobName,
                        Resource = "b",
                        ExpiresOn = DateTimeOffset.UtcNow.AddYears(10)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    azureUrl = blobClient.GenerateSasUri(sasBuilder).ToString();
                }
                else
                {
                    azureUrl = blobClient.Uri.ToString();
                }

                _logger.LogInformation("Uploaded file {FileName} to Azure Blob Storage: {Url}", file.FileName, azureUrl);
                return azureUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName} to Azure Blob Storage: {Message}. Falling back to Cloudinary...", file.FileName, ex.Message);
            }
        }

        // 2. Try Cloudinary if Azure is not available or failed
        if (_cloudinary != null)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var contentType = file.ContentType?.ToLower() ?? "";
                var ext = Path.GetExtension(file.FileName).ToLower();
                var isPdf = ext == ".pdf" || contentType == "application/pdf";
                var isImage = contentType.StartsWith("image/") || isPdf;
                var isVideo = contentType.StartsWith("video/");

                if (isImage)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"mess_chat/{folder}",
                        PublicId = isPdf ? $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(file.FileName)}" : $"{Guid.NewGuid():N}",
                        UseFilename = true,
                        UniqueFilename = true,
                        AccessMode = "public"
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        _logger.LogInformation("Uploaded image/pdf {FileName} to Cloudinary: {Url}", file.FileName, url);
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
                        UniqueFilename = true,
                        AccessMode = "public"
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
                        UniqueFilename = true,
                        AccessMode = "public"
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

        // 3. Fallback to local storage if all cloud services fail or are unconfigured
        return await _fallbackStorage.SaveFileAsync(file, folder);
    }
}
