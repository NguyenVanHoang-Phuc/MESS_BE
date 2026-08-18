using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.RemoveParticipant;

public class RemoveParticipantCommand : IRequest<Result<ConversationResponse>>
{
    public Guid ConversationId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid UserIdToRemove { get; set; }
}
