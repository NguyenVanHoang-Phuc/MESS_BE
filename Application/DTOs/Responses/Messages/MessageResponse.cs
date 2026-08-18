namespace MESS.Application.DTOs.Responses.Messages;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsRecalled { get; set; }
    public DateTime SentAt { get; set; }
    public List<AttachmentResponse> Attachments { get; set; } = new();
    public List<ReactionResponse> Reactions { get; set; } = new();
}

public class AttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

public class ReactionResponse
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> UserNames { get; set; } = new();
}
