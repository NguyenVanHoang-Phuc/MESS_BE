using MESS.Application.UseCases.Files.Commands.UploadFiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Authorize]
public class FilesController : ApiControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
    {
        var command = new UploadFilesCommand { Files = files };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
