using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Users.Queries.GetAllUsers;
using MESS.Application.UseCases.Users.Queries.SearchUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public UsersController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllUsersQuery());
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int limit = 20)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new SearchUsersQuery(
            q ?? string.Empty,
            currentUserId,
            limit
        ));
        return HandleResult(result);
    }
}
