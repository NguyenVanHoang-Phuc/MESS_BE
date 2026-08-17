using System;

namespace MESS.Domain.Shared;

public abstract class BaseEntity
{
}

public abstract class Entity : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
