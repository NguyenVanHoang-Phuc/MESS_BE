using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class MessageReaction : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string EmojiCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Message? Message { get; set; }
    public virtual User? User { get; set; }
}
