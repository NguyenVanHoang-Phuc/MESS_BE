using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Conversations.Commands.CreateConversation;
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
}
