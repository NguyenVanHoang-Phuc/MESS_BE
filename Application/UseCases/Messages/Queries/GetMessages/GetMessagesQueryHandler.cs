using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MESS.Application.Common.Models;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<CursorPaginatedResponse<MessageResponse>>>
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

    public async Task<Result<CursorPaginatedResponse<MessageResponse>>> Handle(
        GetMessagesQuery request, CancellationToken cancellationToken)
    {
        // 1. Check access
        var isParticipant = await _conversationRepository.IsParticipantAsync(request.ConversationId, request.RequesterId);
        if (!isParticipant)
            return Result<CursorPaginatedResponse<MessageResponse>>.Failure(DomainErrors.Conversation.AccessDenied);

        var limit = request.Limit > 0 ? Math.Min(request.Limit, 100) : (request.PageSize ?? 30);

        // 2. Fetch limit + 1 items to determine if HasMore
        var rawMessages = await _messageRepository.GetConversationMessagesByCursorAsync(
            request.ConversationId, request.BeforeCursor, limit + 1);

        bool hasMore = rawMessages.Count > limit;
        if (hasMore)
        {
            rawMessages.RemoveAt(rawMessages.Count - 1);
        }

        var totalCount = await _messageRepository.GetConversationMessageCountAsync(request.ConversationId);

        // Next cursor is the oldest message's CreatedAt timestamp in this batch
        DateTime? nextCursor = rawMessages.Count > 0 ? rawMessages.Last().CreatedAt : null;

        // 3. Order ascending (chronological) for the chat UI
        var mappedMessages = _mapper.Map<List<MessageResponse>>(rawMessages)
            .OrderBy(m => m.SentAt)
            .ToList();

        var response = new CursorPaginatedResponse<MessageResponse>(
            mappedMessages,
            nextCursor,
            hasMore,
            totalCount
        );

        return Result<CursorPaginatedResponse<MessageResponse>>.Success(response);
    }
}
