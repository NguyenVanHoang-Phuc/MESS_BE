using System;
using System.Collections.Generic;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class WorkShift : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DefaultConversationId { get; set; }

    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual Conversation? DefaultConversation { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
