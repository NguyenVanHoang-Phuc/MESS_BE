using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Attachment : AuditableEntity
{
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public int? FileSize { get; set; }

    // Navigation properties
    public virtual Message? Message { get; set; }
}
