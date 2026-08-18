using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.SearchMessages;

public record SearchMessagesQuery(
    Guid CurrentUserId,
    string? Keyword = null,
    Guid? SenderId = null,
    Guid? ConversationId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool? HasAttachments = null,
    string? FileType = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<MessageSearchPagedResponse>>;
