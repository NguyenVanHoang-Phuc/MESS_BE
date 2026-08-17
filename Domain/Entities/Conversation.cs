using System;
using System.Collections.Generic;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Conversation : AuditableEntity
{
    public string? Title { get; set; }
    public string Type { get; set; } = string.Empty; // Direct or Group
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public virtual User? Creator { get; set; }
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
