namespace MESS.Application.DTOs.Responses.Messages;

public class MessageSearchResultResponse
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public string? ConversationTitle { get; set; }
    public string ConversationType { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderUsername { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
    public List<AttachmentResponse> Attachments { get; set; } = new();
}

public class MessageSearchPagedResponse
{
    public List<MessageSearchResultResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
