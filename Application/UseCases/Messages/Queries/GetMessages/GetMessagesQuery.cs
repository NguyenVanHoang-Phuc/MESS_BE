using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Common.Models;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.GetMessages;

public class GetMessagesQuery : IRequest<Result<PaginatedList<MessageResponse>>>
{
    public Guid ConversationId { get; set; }
    public Guid RequesterId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}
