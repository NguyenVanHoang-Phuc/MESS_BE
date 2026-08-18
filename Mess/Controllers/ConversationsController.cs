using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Conversations.Commands.AddParticipant;
using MESS.Application.UseCases.Conversations.Commands.CreateConversation;
using MESS.Application.UseCases.Conversations.Commands.DeleteConversation;
using MESS.Application.UseCases.Conversations.Commands.RemoveParticipant;
using MESS.Application.UseCases.Conversations.Queries.GetMyConversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Authorize]
public class ConversationsController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ConversationsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var query = new GetMyConversationsQuery { UserId = _currentUser.UserId!.Value };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationCommand command)
    {
        command.CreatorId = _currentUser.UserId!.Value;
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{conversationId:guid}/participants")]
    public async Task<IActionResult> AddParticipant(Guid conversationId, [FromBody] AddParticipantRequest request)
    {
        var command = new AddParticipantCommand
        {
            ConversationId = conversationId,
            RequesterId = _currentUser.UserId!.Value,
            UserIdToAdd = request.UserId,
            Role = request.Role ?? "Member"
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{conversationId:guid}/participants/{userId:guid}")]
    public async Task<IActionResult> RemoveParticipant(Guid conversationId, Guid userId)
    {
        var command = new RemoveParticipantCommand
        {
            ConversationId = conversationId,
            RequesterId = _currentUser.UserId!.Value,
            UserIdToRemove = userId
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{conversationId:guid}")]
    public async Task<IActionResult> Delete(Guid conversationId)
    {
        var command = new DeleteConversationCommand
        {
            ConversationId = conversationId,
            RequesterId = _currentUser.UserId!.Value
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public class AddParticipantRequest
{
    public Guid UserId { get; set; }
    public string? Role { get; set; } = "Member";
}
