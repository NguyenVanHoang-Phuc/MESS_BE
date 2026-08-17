using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Task : AuditableEntity
{
    public Guid? SourceMessageId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Todo"; // Todo, InProgress, Done
    public string? RefType { get; set; } // Polymorphic: Lệnh SX, Ticket...
    public string? RefId { get; set; }

    // Navigation properties
    public virtual Message? SourceMessage { get; set; }
    public virtual User? Assignee { get; set; }
    public virtual User? Creator { get; set; }
}
