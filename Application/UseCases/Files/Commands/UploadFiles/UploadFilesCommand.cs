using MediatR;
using MESS.Application.DTOs.Responses.Files;
using MESS.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace MESS.Application.UseCases.Files.Commands.UploadFiles;

public class UploadFilesCommand : IRequest<Result<List<FileUploadResponse>>>
{
    public List<IFormFile> Files { get; set; } = new();
}
