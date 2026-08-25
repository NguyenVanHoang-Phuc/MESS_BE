using System;
using System.Collections.Generic;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Conversation : AuditableEntity
{
    public string? Title { get; set; }
    public string Type { get; set; } = string.Empty; // Direct or Group
    public string? AvatarUrl { get; set; }
    public string? CanonicalKey { get; set; }

    // MES-015: System Org Groups
    public bool IsSystemGroup { get; set; } = false;
    public Guid? DepartmentId { get; set; }
    public Guid? WorkShiftId { get; set; }

    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual WorkShift? WorkShift { get; set; }
    public virtual User? Creator { get; set; }
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
