using System;
using System.Collections.Generic;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual Role? Role { get; set; }
    
    public virtual ICollection<Conversation> CreatedConversations { get; set; } = new List<Conversation>();
    public virtual ICollection<Participant> Participations { get; set; } = new List<Participant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    public virtual ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();
    
    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public virtual ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
}
