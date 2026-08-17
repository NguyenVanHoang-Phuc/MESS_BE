using System;
using System.Collections.Generic;
using MESS.Domain.Shared;
using MESS.Domain.Interfaces;

namespace MESS.Domain.Entities;

public class Message : AuditableEntity, ISoftDelete
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public bool IsRecalled { get; set; } = false;
    
    // ISoftDelete implementation
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation properties
    public virtual Conversation? Conversation { get; set; }
    public virtual User? Sender { get; set; }
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    public virtual ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();
    
    // One-to-one relationship with Task
    public virtual Task? Task { get; set; }
}
