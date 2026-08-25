using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MESS.Application.DTOs.Requests.Org;
using MESS.Application.DTOs.Responses.Org;
using MESS.Application.Interfaces.Org;
using MESS.Domain.Entities;
using MESS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace MESS.Mess.Controllers;

[Authorize]
[Route("api/admin")]
[ApiController]
public class AdminOrgController : ControllerBase
{
    private readonly MessDbContext _context;
    private readonly IOrgSyncService _orgSyncService;

    public AdminOrgController(MessDbContext context, IOrgSyncService orgSyncService)
    {
        _context = context;
        _orgSyncService = orgSyncService;
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var totalDepts = await _context.Departments.CountAsync();
        var totalShifts = await _context.WorkShifts.CountAsync();
        var totalOrgGroups = await _context.Conversations.CountAsync(c => c.IsSystemGroup);
        var totalTasks = await _context.Tasks.CountAsync();

        var breakdown = await _context.Departments
            .Include(d => d.Users)
            .Select(d => new DepartmentStatsDto
            {
                DepartmentId = d.Id,
                DepartmentName = d.Name,
                UserCount = d.Users.Count(u => u.IsActive)
            })
            .OrderByDescending(d => d.UserCount)
            .ToListAsync();

        var stats = new AdminDashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersCount = activeUsers,
            TotalDepartments = totalDepts,
            TotalWorkShifts = totalShifts,
            TotalOrgGroups = totalOrgGroups,
            TotalTasks = totalTasks,
            DepartmentBreakdown = breakdown
        };

        return Ok(new { success = true, data = stats });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAdminUsers(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? workShiftId,
        [FromQuery] Guid? roleId,
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

        if (departmentId.HasValue) query = query.Where(u => u.DepartmentId == departmentId.Value);
        if (workShiftId.HasValue) query = query.Where(u => u.WorkShiftId == workShiftId.Value);
        if (roleId.HasValue) query = query.Where(u => u.RoleId == roleId.Value);
        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);

        var users = await query
            .OrderBy(u => u.FullName)
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

    [HttpPut("users/{id}/org")]
    public async Task<IActionResult> UpdateUserOrg(Guid id, [FromBody] UpdateUserOrgRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

        var oldDeptId = user.DepartmentId;
        var oldShiftId = user.WorkShiftId;

        if (request.DepartmentId.HasValue || request.DepartmentId == null)
        {
            user.DepartmentId = request.DepartmentId;
        }

        if (request.WorkShiftId.HasValue || request.WorkShiftId == null)
        {
            user.WorkShiftId = request.WorkShiftId;
        }

        if (request.PositionTitle != null)
        {
            user.PositionTitle = request.PositionTitle.Trim();
        }

        if (request.RoleId.HasValue)
        {
            user.RoleId = request.RoleId.Value;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
            if (!user.IsActive)
            {
                await _orgSyncService.OnUserDeactivatedAsync(user.Id);
            }
        }

        await _context.SaveChangesAsync();

        // Trigger dynamic group synchronization when department or shift changed
        if (user.IsActive && (oldDeptId != user.DepartmentId || oldShiftId != user.WorkShiftId))
        {
            await _orgSyncService.OnUserDepartmentOrShiftChangedAsync(user.Id, oldDeptId, user.DepartmentId, oldShiftId, user.WorkShiftId);
        }

        return Ok(new { success = true, message = "Cập nhật thông tin tổ chức nhân viên thành công!" });
    }

    [HttpPost("org/sync-groups")]
    public async Task<IActionResult> SyncOrgGroups()
    {
        var result = await _orgSyncService.SyncAllOrgGroupsAsync();
        return Ok(new { success = true, data = result });
    }

    [HttpPost("org/departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "Tên phòng ban không được để trống." });

        Guid? parentId = request.ParentDepartmentId;
        if (parentId == Guid.Empty) parentId = null;

        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Code = request.Code?.Trim() ?? string.Empty,
            ParentDepartmentId = parentId,
            AutoCreateGroup = request.AutoCreateGroup,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Departments.AddAsync(dept);
        await _context.SaveChangesAsync();

        if (dept.AutoCreateGroup)
        {
            await _orgSyncService.EnsureDepartmentGroupAsync(dept.Id);
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                dept.Id,
                dept.Name,
                dept.Code,
                dept.ParentDepartmentId,
                dept.AutoCreateGroup,
                dept.DefaultConversationId,
                dept.CreatedAt
            }
        });
    }

    [HttpPut("org/departments/{id}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return NotFound(new { success = false, message = "Không tìm thấy phòng ban." });

        Guid? parentId = request.ParentDepartmentId;
        if (parentId == Guid.Empty) parentId = null;

        dept.Name = request.Name.Trim();
        dept.Code = request.Code?.Trim() ?? string.Empty;
        dept.ParentDepartmentId = parentId;
        dept.AutoCreateGroup = request.AutoCreateGroup;

        await _context.SaveChangesAsync();

        if (dept.AutoCreateGroup)
        {
            await _orgSyncService.EnsureDepartmentGroupAsync(dept.Id);
        }

        return Ok(new { success = true, message = "Cập nhật phòng ban thành công!" });
    }

    [HttpPost("org/shifts")]
    public async Task<IActionResult> CreateWorkShift([FromBody] CreateWorkShiftRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "Tên ca làm việc không được để trống." });

        Guid? deptId = request.DepartmentId;
        if (deptId == Guid.Empty) deptId = null;

        if (deptId.HasValue)
        {
            var deptExists = await _context.Departments.AnyAsync(d => d.Id == deptId.Value);
            if (!deptExists) deptId = null;
        }

        var shift = new WorkShift
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Code = request.Code?.Trim() ?? string.Empty,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DepartmentId = deptId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.WorkShifts.AddAsync(shift);
        await _context.SaveChangesAsync();

        await _orgSyncService.EnsureWorkShiftGroupAsync(shift.Id);

        return Ok(new
        {
            success = true,
            data = new
            {
                shift.Id,
                shift.Name,
                shift.Code,
                shift.StartTime,
                shift.EndTime,
                shift.DepartmentId,
                shift.DefaultConversationId,
                shift.CreatedAt
            }
        });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles.AsNoTracking().ToListAsync();
        return Ok(new { success = true, data = roles });
    }
}
