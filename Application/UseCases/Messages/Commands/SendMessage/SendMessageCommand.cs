using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.SendMessage;

public class AttachmentInputDto
{
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public int? FileSize { get; set; }
}

public class SendMessageCommand : IRequest<Result<MessageResponse>>
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public List<AttachmentInputDto> Attachments { get; set; } = new();
}
