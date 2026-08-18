using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Messages.Commands.SendMessage;
using MESS.Application.UseCases.Messages.Queries.GetMessages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Authorize]
public class MessagesController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;

    public MessagesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 30)
    {
        var query = new GetMessagesQuery
        {
            ConversationId = conversationId,
            RequesterId = _currentUser.UserId!.Value,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageCommand command)
    {
        command.SenderId = _currentUser.UserId!.Value;
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
