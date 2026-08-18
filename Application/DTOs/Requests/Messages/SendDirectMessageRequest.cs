using MESS.Application.UseCases.Messages.Commands.SendMessage;

namespace MESS.Application.DTOs.Requests.Messages;

public class SendDirectMessageRequest
{
    public Guid RecipientId { get; set; }
    public string? Content { get; set; }
    public List<AttachmentInputDto>? Attachments { get; set; }
    public string? ClientOperationId { get; set; }
}
