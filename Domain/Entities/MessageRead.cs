using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class MessageRead : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Message? Message { get; set; }
    public virtual User? User { get; set; }
}
