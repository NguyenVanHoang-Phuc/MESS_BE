using System;
using System.Collections.Generic;

namespace MESS.Application.DTOs.Responses.Tasks;

public class TaskAssigneeDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public List<TaskAssigneeDto> Assignees { get; set; } = new();
    public List<Guid> AssigneeIds { get; set; } = new();
    public Guid? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public Guid? SourceMessageId { get; set; }
    public Guid? ConversationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
