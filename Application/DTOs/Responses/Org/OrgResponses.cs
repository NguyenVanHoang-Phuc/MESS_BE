using System;
using System.Collections.Generic;

namespace MESS.Application.DTOs.Responses.Org;

public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public bool AutoCreateGroup { get; set; }
    public Guid? DefaultConversationId { get; set; }
    public int UserCount { get; set; }
    public List<DirectoryUserDto> Users { get; set; } = new List<DirectoryUserDto>();
    public List<DepartmentTreeDto> SubDepartments { get; set; } = new List<DepartmentTreeDto>();
}

public class DirectoryUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public Guid? WorkShiftId { get; set; }
    public string? WorkShiftName { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? PositionTitle { get; set; }
    public bool IsActive { get; set; }
}

public class WorkShiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DefaultConversationId { get; set; }
    public int UserCount { get; set; }
    public List<DirectoryUserDto> Users { get; set; } = new List<DirectoryUserDto>();
}

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalWorkShifts { get; set; }
    public int TotalOrgGroups { get; set; }
    public int ActiveUsersCount { get; set; }
    public int TotalTasks { get; set; }
    public List<DepartmentStatsDto> DepartmentBreakdown { get; set; } = new List<DepartmentStatsDto>();
}

public class DepartmentStatsDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class SyncOrgResultDto
{
    public int CreatedGroupsCount { get; set; }
    public int UpdatedMembersCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
