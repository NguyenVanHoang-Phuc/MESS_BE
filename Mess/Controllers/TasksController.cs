using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Tasks.Commands.AssignTask;
using MESS.Application.UseCases.Tasks.Commands.CreateTask;
using MESS.Application.UseCases.Tasks.Commands.DeleteTask;
using MESS.Application.UseCases.Tasks.Commands.UpdateTaskStatus;
using MESS.Application.UseCases.Tasks.Queries.GetTaskById;
using MESS.Application.UseCases.Tasks.Queries.GetTasks;
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

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? conversationId,
        [FromQuery] Guid? messageId,
        [FromQuery] Guid? assigneeId,
        [FromQuery] Guid? creatorId,
        [FromQuery] string? status)
    {
        var query = new GetTasksQuery
        {
            ConversationId = conversationId,
            MessageId = messageId,
            AssigneeId = assigneeId,
            CreatorId = creatorId,
            Status = status
        };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTaskByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTaskRequest request)
    {
        var command = new AssignTaskCommand
        {
            TaskId = id,
            AssigneeId = request.AssigneeId,
            AssigneeIds = request.AssigneeIds,
            CurrentUserId = _currentUser.UserId!.Value
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var command = new UpdateTaskStatusCommand
        {
            TaskId = id,
            Status = request.Status,
            CurrentUserId = _currentUser.UserId!.Value
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTaskCommand
        {
            TaskId = id,
            CurrentUserId = _currentUser.UserId!.Value
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public class AssignTaskRequest
{
    public Guid? AssigneeId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
}

public class UpdateTaskStatusRequest
{
    public string Status { get; set; } = "Todo";
}
