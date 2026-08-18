namespace MESS.Application.DTOs.Requests.Messages;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
}

public class CreateTaskFromMessageRequest
{
    public Guid MessageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? Deadline { get; set; }
}
