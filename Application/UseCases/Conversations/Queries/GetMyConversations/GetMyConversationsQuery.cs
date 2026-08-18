using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Queries.GetMyConversations;

public class GetMyConversationsQuery : IRequest<Result<IEnumerable<ConversationResponse>>>
{
    public Guid UserId { get; set; }
}
