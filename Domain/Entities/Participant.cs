using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Participant : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty; // Admin or Member
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Conversation? Conversation { get; set; }
    public virtual User? User { get; set; }
}
