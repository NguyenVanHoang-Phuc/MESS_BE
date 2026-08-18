using MediatR;
using MESS.Application.DTOs.Responses.Files;
using MESS.Application.Interfaces.Storage;
using MESS.Domain.Errors;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Files.Commands.UploadFiles;

public class UploadFilesCommandHandler : IRequestHandler<UploadFilesCommand, Result<List<FileUploadResponse>>>
{
    private readonly IFileStorageService _fileStorageService;
    private const long MaxFileSize = 25 * 1024 * 1024; // 25 MB
    private const int MaxFileCount = 30;

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mov", ".avi", ".mkv"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".ogg", ".m4a"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".zip", ".rar", ".7z", ".json"
    };

    public UploadFilesCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<List<FileUploadResponse>>> Handle(UploadFilesCommand request, CancellationToken cancellationToken)
    {
        if (request.Files == null || request.Files.Count == 0)
            return Result<List<FileUploadResponse>>.Failure(DomainErrors.File.Empty);

        if (request.Files.Count > MaxFileCount)
            return Result<List<FileUploadResponse>>.Failure(DomainErrors.File.TooManyFiles);

        // Validate all files before saving
        foreach (var file in request.Files)
        {
            if (file.Length == 0)
                return Result<List<FileUploadResponse>>.Failure(DomainErrors.File.Empty);

            if (file.Length > MaxFileSize)
                return Result<List<FileUploadResponse>>.Failure(DomainErrors.File.TooLarge);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || (!ImageExtensions.Contains(ext) && !VideoExtensions.Contains(ext) && !AudioExtensions.Contains(ext) && !DocumentExtensions.Contains(ext)))
            {
                return Result<List<FileUploadResponse>>.Failure(DomainErrors.File.InvalidFormat);
            }
        }

        var results = new List<FileUploadResponse>();

        foreach (var file in request.Files)
        {
            var ext = Path.GetExtension(file.FileName);
            string folder = "documents";
            if (ImageExtensions.Contains(ext)) folder = "images";
            else if (VideoExtensions.Contains(ext)) folder = "videos";
            else if (AudioExtensions.Contains(ext)) folder = "audios";

            var fileUrl = await _fileStorageService.SaveFileAsync(file, folder);

            var rawContentType = file.ContentType ?? "application/octet-stream";
            var safeContentType = rawContentType;
            if (safeContentType.Length > 50)
            {
                if (safeContentType.Contains("spreadsheetml")) safeContentType = "application/vnd.ms-excel";
                else if (safeContentType.Contains("wordprocessingml")) safeContentType = "application/msword";
                else if (safeContentType.Contains("presentationml")) safeContentType = "application/vnd.ms-powerpoint";
                else safeContentType = safeContentType.Substring(0, 50);
            }

            results.Add(new FileUploadResponse
            {
                FileName = file.FileName,
                FileUrl = fileUrl,
                FileType = safeContentType,
                FileSize = file.Length
            });
        }

        return Result<List<FileUploadResponse>>.Success(results);
    }
}
