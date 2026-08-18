using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.SearchMessages;

public class SearchMessagesQueryHandler : IRequestHandler<SearchMessagesQuery, Result<MessageSearchPagedResponse>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public SearchMessagesQueryHandler(IMessageRepository messageRepository, IMapper mapper)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<Result<MessageSearchPagedResponse>> Handle(
        SearchMessagesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _messageRepository.SearchMessagesAsync(
            request.CurrentUserId,
            request.Keyword,
            request.SenderId,
            request.ConversationId,
            request.FromDate,
            request.ToDate,
            request.HasAttachments,
            request.FileType,
            request.PageNumber,
            request.PageSize
        );

        var resultItems = items.Select(m => new MessageSearchResultResponse
        {
            MessageId = m.Id,
            ConversationId = m.ConversationId,
            ConversationTitle = m.Conversation?.Title ?? (m.Conversation?.Type == "Direct" ? m.Conversation?.Participants.FirstOrDefault(p => p.UserId != request.CurrentUserId)?.User?.FullName : "Hội thoại"),
            ConversationType = m.Conversation?.Type ?? "Direct",
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName ?? "Unknown",
            SenderUsername = m.Sender?.Username,
            Content = m.Content,
            SentAt = m.CreatedAt,
            Attachments = _mapper.Map<List<AttachmentResponse>>(m.Attachments)
        }).ToList();

        var response = new MessageSearchPagedResponse
        {
            Items = resultItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<MessageSearchPagedResponse>.Success(response);
    }
}
