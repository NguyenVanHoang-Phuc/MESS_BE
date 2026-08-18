using MESS.Application.Interfaces.Auth;
using MESS.Application.UseCases.Messages.Commands.MarkConversationAsRead;
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
        [FromQuery] DateTime? beforeCursor = null,
        [FromQuery] int limit = 30,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var query = new GetMessagesQuery
        {
            ConversationId = conversationId,
            RequesterId = _currentUser.UserId!.Value,
            BeforeCursor = beforeCursor,
            Limit = limit,
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

    [HttpPost("direct")]
    public async Task<IActionResult> SendDirect([FromBody] MESS.Application.DTOs.Requests.Messages.SendDirectMessageRequest request)
    {
        var command = new MESS.Application.UseCases.Messages.Commands.SendDirectMessage.SendDirectMessageCommand
        {
            SenderId = _currentUser.UserId!.Value,
            RecipientId = request.RecipientId,
            Content = request.Content,
            Attachments = request.Attachments ?? new(),
            ClientOperationId = request.ClientOperationId
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] Guid? senderId,
        [FromQuery] Guid? conversationId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] bool? hasAttachments,
        [FromQuery] string? fileType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new MESS.Application.UseCases.Messages.Queries.SearchMessages.SearchMessagesQuery(
            CurrentUserId: _currentUser.UserId!.Value,
            Keyword: q,
            SenderId: senderId,
            ConversationId: conversationId,
            FromDate: fromDate,
            ToDate: toDate,
            HasAttachments: hasAttachments,
            FileType: fileType,
            PageNumber: pageNumber,
            PageSize: pageSize
        );
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var command = new MarkConversationAsReadCommand
        {
            ConversationId = conversationId,
            ReaderId = _currentUser.UserId!.Value
        };
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{messageId:guid}/recall")]
    [HttpPut("{messageId:guid}/recall")]
    public async Task<IActionResult> Recall(Guid messageId)
    {
        var command = new MESS.Application.UseCases.Messages.Commands.RecallMessage.RecallMessageCommand(
            messageId,
            _currentUser.UserId!.Value
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{messageId:guid}/react")]
    public async Task<IActionResult> React(Guid messageId, [FromBody] MESS.Application.DTOs.Requests.Messages.ReactMessageRequest request)
    {
        var command = new MESS.Application.UseCases.Messages.Commands.ReactMessage.ReactMessageCommand(
            messageId,
            _currentUser.UserId!.Value,
            request.Emoji
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
