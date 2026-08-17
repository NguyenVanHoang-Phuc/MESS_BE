using System;
using System.Collections.Generic;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
