using MediatR;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.MarkConversationAsRead;

public class MarkConversationAsReadCommand : IRequest<Result>
{
    public Guid ConversationId { get; set; }
    public Guid ReaderId { get; set; }
}
