using System;

namespace MESS.Application.DTOs.Responses.Tasks;

public class TaskReminderDto
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public string Type { get; set; } = "DueSoon1h"; // "DueSoon24h" | "DueSoon1h" | "Overdue"
    public DateTime Deadline { get; set; }
    public string Message { get; set; } = string.Empty;
}
