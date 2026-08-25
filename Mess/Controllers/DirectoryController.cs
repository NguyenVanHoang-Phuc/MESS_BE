using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MESS.Application.DTOs.Responses.Org;
using MESS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MESS.Mess.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DirectoryController : ControllerBase
{
    private readonly MessDbContext _context;

    public DirectoryController(MessDbContext context)
    {
        _context = context;
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetOrgTree()
    {
        var departments = await _context.Departments
            .Include(d => d.Users.Where(u => u.IsActive))
            .ThenInclude(u => u.Role)
            .Include(d => d.Users.Where(u => u.IsActive))
            .ThenInclude(u => u.WorkShift)
            .AsNoTracking()
            .ToListAsync();

        var rootDepartments = departments.Where(d => d.ParentDepartmentId == null).ToList();

        List<DepartmentTreeDto> BuildTree(List<MESS.Domain.Entities.Department> depts, Guid? parentId)
        {
            return depts
                .Where(d => d.ParentDepartmentId == parentId)
                .Select(d => new DepartmentTreeDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    ParentDepartmentId = d.ParentDepartmentId,
                    AutoCreateGroup = d.AutoCreateGroup,
                    DefaultConversationId = d.DefaultConversationId,
                    UserCount = d.Users.Count,
                    Users = d.Users.Select(u => new DirectoryUserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FullName = u.FullName,
                        DepartmentId = d.Id,
                        DepartmentName = d.Name,
                        DepartmentCode = d.Code,
                        WorkShiftId = u.WorkShiftId,
                        WorkShiftName = u.WorkShift?.Name,
                        RoleId = u.RoleId,
                        RoleName = u.Role?.Name,
                        PositionTitle = u.PositionTitle,
                        IsActive = u.IsActive
                    }).ToList(),
                    SubDepartments = BuildTree(departments, d.Id)
                })
                .ToList();
        }

        var result = BuildTree(departments, null);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetDirectoryUsers(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? workShiftId,
        [FromQuery] bool? isActive)
    {
        var query = _context.Users
            .Include(u => u.Department)
            .Include(u => u.WorkShift)
            .Include(u => u.Role)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Username.ToLower().Contains(s) || (u.PositionTitle != null && u.PositionTitle.ToLower().Contains(s)));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == departmentId.Value);
        }

        if (workShiftId.HasValue)
        {
            query = query.Where(u => u.WorkShiftId == workShiftId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var users = await query
            .OrderBy(u => u.Department != null ? u.Department.Name : "zzz")
            .ThenBy(u => u.FullName)
            .Select(u => new DirectoryUserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : null,
                DepartmentCode = u.Department != null ? u.Department.Code : null,
                WorkShiftId = u.WorkShiftId,
                WorkShiftName = u.WorkShift != null ? u.WorkShift.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null,
                PositionTitle = u.PositionTitle,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return Ok(new { success = true, data = users });
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.Departments
            .Include(d => d.Users)
            .AsNoTracking()
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Code,
                d.ParentDepartmentId,
                d.AutoCreateGroup,
                d.DefaultConversationId,
                UserCount = d.Users.Count(u => u.IsActive)
            })
            .OrderBy(d => d.Name)
            .ToListAsync();

        return Ok(new { success = true, data = departments });
    }

    [HttpGet("shifts")]
    public async Task<IActionResult> GetWorkShifts()
    {
        var shifts = await _context.WorkShifts
            .Include(ws => ws.Department)
            .Include(ws => ws.Users)
            .AsNoTracking()
            .Select(ws => new WorkShiftDto
            {
                Id = ws.Id,
                Name = ws.Name,
                Code = ws.Code,
                StartTime = ws.StartTime,
                EndTime = ws.EndTime,
                DepartmentId = ws.DepartmentId,
                DepartmentName = ws.Department != null ? ws.Department.Name : null,
                DefaultConversationId = ws.DefaultConversationId,
                UserCount = ws.Users.Count(u => u.IsActive)
            })
            .OrderBy(ws => ws.StartTime)
            .ToListAsync();

        return Ok(new { success = true, data = shifts });
    }
}
