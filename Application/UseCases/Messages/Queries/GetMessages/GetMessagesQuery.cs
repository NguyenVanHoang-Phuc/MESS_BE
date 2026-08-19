using System;
using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Common.Models;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.GetMessages;

public class GetMessagesQuery : IRequest<Result<CursorPaginatedResponse<MessageResponse>>>
{
    public Guid ConversationId { get; set; }
    public Guid RequesterId { get; set; }
    public DateTime? BeforeCursor { get; set; }
    public int Limit { get; set; } = 30;
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
