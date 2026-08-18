using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Tasks.Commands.CreateTask;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Authorize]
public class TasksController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public TasksController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command)
    {
        command.CreatorId = _currentUser.UserId!.Value;
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
