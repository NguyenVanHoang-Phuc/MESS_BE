using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using MESS.Application.DTOs.Responses.Org;
using MESS.Application.Interfaces.Notifications;
using MESS.Application.Interfaces.Org;
using MESS.Domain.Entities;
using MESS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MESS.Infrastructure.Services;

public class OrgSyncService : IOrgSyncService
{
    private readonly MessDbContext _context;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly ILogger<OrgSyncService> _logger;

    public OrgSyncService(
        MessDbContext context,
        IChatNotificationService chatNotificationService,
        ILogger<OrgSyncService> logger)
    {
        _context = context;
        _chatNotificationService = chatNotificationService;
        _logger = logger;
    }

    public async Task EnsureDepartmentGroupAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments
            .Include(d => d.Users.Where(u => u.IsActive))
            .Include(d => d.DefaultConversation)
            .ThenInclude(c => c!.Participants)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (dept == null || !dept.AutoCreateGroup) return;

        Conversation? conversation = dept.DefaultConversation;

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Title = $"[Phòng] {dept.Name}",
                Type = "Group",
                IsSystemGroup = true,
                DepartmentId = dept.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation, cancellationToken);
            dept.DefaultConversationId = conversation.Id;
        }
        else
        {
            conversation.Title = $"[Phòng] {dept.Name}";
            conversation.IsSystemGroup = true;
            conversation.DepartmentId = dept.Id;
        }

        // Add missing active members
        var currentParticipantUserIds = conversation.Participants.Select(p => p.UserId).ToHashSet();
        foreach (var user in dept.Users)
        {
            if (!currentParticipantUserIds.Contains(user.Id))
            {
                conversation.Participants.Add(new Participant
                {
                    ConversationId = conversation.Id,
                    UserId = user.Id,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        // Remove users who are no longer in this department
        var deptUserIds = dept.Users.Select(u => u.Id).ToHashSet();
        var participantsToRemove = conversation.Participants
            .Where(p => !deptUserIds.Contains(p.UserId))
            .ToList();

        foreach (var p in participantsToRemove)
        {
            conversation.Participants.Remove(p);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ensured department group for {DeptName} (ID: {DeptId}, ConvID: {ConvId})", dept.Name, dept.Id, conversation.Id);
    }

    public async Task EnsureWorkShiftGroupAsync(Guid workShiftId, CancellationToken cancellationToken = default)
    {
        var shift = await _context.WorkShifts
            .Include(ws => ws.Users.Where(u => u.IsActive))
            .Include(ws => ws.DefaultConversation)
            .ThenInclude(c => c!.Participants)
            .FirstOrDefaultAsync(ws => ws.Id == workShiftId, cancellationToken);

        if (shift == null) return;

        Conversation? conversation = shift.DefaultConversation;

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Title = $"[Ca trực] {shift.Name}",
                Type = "Group",
                IsSystemGroup = true,
                WorkShiftId = shift.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation, cancellationToken);
            shift.DefaultConversationId = conversation.Id;
        }
        else
        {
            conversation.Title = $"[Ca trực] {shift.Name}";
            conversation.IsSystemGroup = true;
            conversation.WorkShiftId = shift.Id;
        }

        // Add missing active members
        var currentParticipantUserIds = conversation.Participants.Select(p => p.UserId).ToHashSet();
        foreach (var user in shift.Users)
        {
            if (!currentParticipantUserIds.Contains(user.Id))
            {
                conversation.Participants.Add(new Participant
                {
                    ConversationId = conversation.Id,
                    UserId = user.Id,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        // Remove users who are no longer in this shift
        var shiftUserIds = shift.Users.Select(u => u.Id).ToHashSet();
        var participantsToRemove = conversation.Participants
            .Where(p => !shiftUserIds.Contains(p.UserId))
            .ToList();

        foreach (var p in participantsToRemove)
        {
            conversation.Participants.Remove(p);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ensured work shift group for {ShiftName} (ID: {ShiftId}, ConvID: {ConvId})", shift.Name, shift.Id, conversation.Id);
    }

    public async Task OnUserDepartmentOrShiftChangedAsync(
        Guid userId,
        Guid? oldDeptId,
        Guid? newDeptId,
        Guid? oldShiftId,
        Guid? newShiftId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) return;

        // Handle Department Change
        if (oldDeptId.HasValue && oldDeptId != newDeptId)
        {
            var oldDept = await _context.Departments
                .Include(d => d.DefaultConversation)
                .ThenInclude(c => c!.Participants)
                .FirstOrDefaultAsync(d => d.Id == oldDeptId.Value, cancellationToken);

            if (oldDept?.DefaultConversation != null)
            {
                var participant = oldDept.DefaultConversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant != null)
                {
                    oldDept.DefaultConversation.Participants.Remove(participant);
                    
                    // Add system message
                    var sysMsg = new Message
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = oldDept.DefaultConversation.Id,
                        SenderId = userId,
                        Content = $"[Hệ thống] {user.FullName} đã được chuyển sang bộ phận khác.",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Messages.AddAsync(sysMsg, cancellationToken);
                }
            }
        }

        if (newDeptId.HasValue && newDeptId != oldDeptId)
        {
            await EnsureDepartmentGroupAsync(newDeptId.Value, cancellationToken);

            var newDept = await _context.Departments
                .Include(d => d.DefaultConversation)
                .ThenInclude(c => c!.Participants)
                .FirstOrDefaultAsync(d => d.Id == newDeptId.Value, cancellationToken);

            if (newDept?.DefaultConversation != null)
            {
                if (!newDept.DefaultConversation.Participants.Any(p => p.UserId == userId))
                {
                    newDept.DefaultConversation.Participants.Add(new Participant
                    {
                        ConversationId = newDept.DefaultConversation.Id,
                        UserId = userId,
                        Role = "Member",
                        JoinedAt = DateTime.UtcNow
                    });

                    // Add system message
                    var sysMsg = new Message
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = newDept.DefaultConversation.Id,
                        SenderId = userId,
                        Content = $"[Hệ thống] Chào mừng {user.FullName} tham gia {newDept.Name}.",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Messages.AddAsync(sysMsg, cancellationToken);
                }
            }
        }

        // Handle WorkShift Change
        if (oldShiftId.HasValue && oldShiftId != newShiftId)
        {
            var oldShift = await _context.WorkShifts
                .Include(ws => ws.DefaultConversation)
                .ThenInclude(c => c!.Participants)
                .FirstOrDefaultAsync(ws => ws.Id == oldShiftId.Value, cancellationToken);

            if (oldShift?.DefaultConversation != null)
            {
                var participant = oldShift.DefaultConversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant != null)
                {
                    oldShift.DefaultConversation.Participants.Remove(participant);
                }
            }
        }

        if (newShiftId.HasValue && newShiftId != oldShiftId)
        {
            await EnsureWorkShiftGroupAsync(newShiftId.Value, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task OnUserDeactivatedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participants = await _context.Participants
            .Include(p => p.Conversation)
            .Where(p => p.UserId == userId && p.Conversation.IsSystemGroup)
            .ToListAsync(cancellationToken);

        if (participants.Count > 0)
        {
            _context.Participants.RemoveRange(participants);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed inactive user {UserId} from {Count} system groups", userId, participants.Count);
        }
    }

    public async Task<SyncOrgResultDto> SyncAllOrgGroupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full organizational groups synchronization...");

        var departments = await _context.Departments
            .Where(d => d.AutoCreateGroup)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        int deptGroupsCreated = 0;
        foreach (var deptId in departments)
        {
            await EnsureDepartmentGroupAsync(deptId, cancellationToken);
            deptGroupsCreated++;
        }

        var shifts = await _context.WorkShifts
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        int shiftGroupsCreated = 0;
        foreach (var shiftId in shifts)
        {
            await EnsureWorkShiftGroupAsync(shiftId, cancellationToken);
            shiftGroupsCreated++;
        }

        return new SyncOrgResultDto
        {
            CreatedGroupsCount = deptGroupsCreated + shiftGroupsCreated,
            UpdatedMembersCount = await _context.Participants.CountAsync(cancellationToken),
            Message = $"Đồng bộ thành công {deptGroupsCreated} nhóm phòng ban và {shiftGroupsCreated} nhóm ca trực."
        };
    }
}
