using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Queries.GetMyConversations;

public class GetMyConversationsQueryHandler : IRequestHandler<GetMyConversationsQuery, Result<IEnumerable<ConversationResponse>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public GetMyConversationsQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ConversationResponse>>> Handle(
        GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(request.UserId);
        var response = _mapper.Map<List<ConversationResponse>>(conversations);

        var conversationIds = response.Select(c => c.Id).ToList();
        if (conversationIds.Count > 0)
        {
            var unreadCounts = await _messageRepository.GetUnreadCountsAsync(conversationIds, request.UserId);

            foreach (var conv in response)
            {
                if (unreadCounts.TryGetValue(conv.Id, out var count))
                {
                    conv.UnreadCount = count;
                }
            }
        }

        return Result<IEnumerable<ConversationResponse>>.Success(response);
    }
}
