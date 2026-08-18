using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Queries.GetMyConversations;

public class GetMyConversationsQueryHandler : IRequestHandler<GetMyConversationsQuery, Result<IEnumerable<ConversationResponse>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;

    public GetMyConversationsQueryHandler(IConversationRepository conversationRepository, IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ConversationResponse>>> Handle(
        GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(request.UserId);
        var response = _mapper.Map<IEnumerable<ConversationResponse>>(conversations);
        return Result<IEnumerable<ConversationResponse>>.Success(response);
    }
}
