using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.AddParticipant;

public class AddParticipantCommand : IRequest<Result<ConversationResponse>>
{
    public Guid ConversationId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid UserIdToAdd { get; set; }
    public string Role { get; set; } = "Member";
}
