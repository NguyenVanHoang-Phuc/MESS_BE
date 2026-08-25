using System;

namespace MESS.Application.DTOs.Requests.Org;

public class UpdateUserOrgRequest
{
    public Guid? DepartmentId { get; set; }
    public Guid? WorkShiftId { get; set; }
    public string? PositionTitle { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public bool AutoCreateGroup { get; set; } = true;
}

public class UpdateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public bool AutoCreateGroup { get; set; } = true;
}

public class CreateWorkShiftRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class UpdateWorkShiftRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? DepartmentId { get; set; }
}
