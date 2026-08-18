using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.CreateConversation;

public class CreateConversationCommand : IRequest<Result<ConversationResponse>>
{
    public Guid CreatorId { get; set; }
    public string Type { get; set; } = "Direct";
    public string? Title { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}
