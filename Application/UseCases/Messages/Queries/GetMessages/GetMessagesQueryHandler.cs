using AutoMapper;
using MediatR;
using MESS.Application.Common.Models;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<PaginatedList<MessageResponse>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<MessageResponse>>> Handle(
        GetMessagesQuery request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền truy cập
        var isParticipant = await _conversationRepository.IsParticipantAsync(request.ConversationId, request.RequesterId);
        if (!isParticipant)
            return Result<PaginatedList<MessageResponse>>.Failure(DomainErrors.Conversation.AccessDenied);

        var messages = await _messageRepository.GetConversationMessagesAsync(
            request.ConversationId, request.PageNumber, request.PageSize);

        var totalCount = await _messageRepository.GetConversationMessageCountAsync(request.ConversationId);
        var mappedMessages = _mapper.Map<List<MessageResponse>>(messages);

        var paginatedList = PaginatedList<MessageResponse>.Create(
            mappedMessages, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<MessageResponse>>.Success(paginatedList);
    }
}
