using MediatR;
using MESS.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator
        => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value });

        return HandleFailure(result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true });

        return HandleFailure(result);
    }

    private IActionResult HandleFailure(Result result)
    {
        var statusCode = GetStatusCode(result.Error.Code);
        return StatusCode(statusCode, new
        {
            success = false,
            statusCode,
            message = result.Error.Message,
            code = result.Error.Code,
            errors = result.Error.ValidationErrors,
            timestamp = DateTime.UtcNow
        });
    }

    private static int GetStatusCode(string errorCode) => errorCode switch
    {
        "Validation.Error" => 422,
        string c when c.Contains(".NotFound") => 404,
        string c when c.Contains(".AccessDenied") => 403,
        string c when c.Contains(".Banned") => 403,
        string c when c.Contains(".Inactive") => 403,
        string c when c.Contains("AlreadyExists") || c.Contains(".Already") => 409,
        string c when c.Contains("Invalid") || c.Contains("Expired") => 400,
        string c when c.StartsWith("User.Invalid") => 401,
        _ => 400
    };
}
