using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.UseCases.Messages.Commands.SendMessage;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.SendDirectMessage;

public class SendDirectMessageCommand : IRequest<Result<SendDirectMessageResponse>>
{
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string? Content { get; set; }
    public List<AttachmentInputDto> Attachments { get; set; } = new();
    public string? ClientOperationId { get; set; }
}
