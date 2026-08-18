using MediatR;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.DeleteConversation;

public class DeleteConversationCommand : IRequest<Result>
{
    public Guid ConversationId { get; set; }
    public Guid RequesterId { get; set; }
}
