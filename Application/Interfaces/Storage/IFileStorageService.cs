using Microsoft.AspNetCore.Http;

namespace MESS.Application.Interfaces.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
}
