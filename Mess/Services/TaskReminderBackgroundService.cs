using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Application.Interfaces.Notifications;
using MESS.Application.UseCases.Tasks;
using MESS.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class TaskReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public TaskReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TaskReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskReminderBackgroundService started.");

        // Initial delay on startup
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTaskRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during task reminder scan.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("TaskReminderBackgroundService stopped.");
    }

    private async Task ProcessTaskRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var chatNotificationService = scope.ServiceProvider.GetRequiredService<IChatNotificationService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var allTasks = await taskRepository.GetAllAsync();
        var nowUtc = DateTime.UtcNow;

        var activeTasksWithDeadline = allTasks
            .Where(t => t.Status != "Done" && t.Deadline.HasValue)
            .ToList();

        if (activeTasksWithDeadline.Count == 0) return;

        foreach (var task in activeTasksWithDeadline)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var deadlineUtc = DateTime.SpecifyKind(task.Deadline!.Value, DateTimeKind.Utc);
            var timeRemaining = deadlineUtc - nowUtc;
            var tags = TaskMetadataHelper.GetReminderTags(task.Description);

            string? reminderType = null;
            string? reminderTagToAdd = null;
            var localDeadline = deadlineUtc.AddHours(7);
            string message = string.Empty;

            if (nowUtc > deadlineUtc)
            {
                // Overdue - remind every 12 hours
                var overdueSlot = $"overdue_{nowUtc:yyyyMMdd}_{(nowUtc.Hour / 12)}";
                if (!tags.Contains(overdueSlot))
                {
                    reminderType = "Overdue";
                    reminderTagToAdd = overdueSlot;
                    message = $"🚨 Công việc '{task.Title}' đã quá hạn hoàn thành lúc {localDeadline:HH:mm dd/MM/yyyy}!";
                }
            }
            else if (timeRemaining <= TimeSpan.FromHours(1) && timeRemaining > TimeSpan.Zero)
            {
                // Due soon in 1 hour
                if (!tags.Contains("due_1h"))
                {
                    reminderType = "DueSoon1h";
                    reminderTagToAdd = "due_1h";
                    message = $"⏰ Công việc '{task.Title}' sắp đến hạn trong vòng 1 giờ tới (Hạn chót: {localDeadline:HH:mm})!";
                }
            }
            else if (timeRemaining <= TimeSpan.FromHours(24) && timeRemaining > TimeSpan.FromHours(1))
            {
                // Due soon in 24 hours
                if (!tags.Contains("due_24h"))
                {
                    reminderType = "DueSoon24h";
                    reminderTagToAdd = "due_24h";
                    message = $"⏰ Công việc '{task.Title}' sắp đến hạn trong vòng 24 giờ tới (Hạn chót: {localDeadline:HH:mm dd/MM})!";
                }
            }

            if (reminderType != null && reminderTagToAdd != null)
            {
                var (cleanDesc, parsedAssigneeIds) = TaskMetadataHelper.ParseDescription(task.Description);
                var recipients = new HashSet<Guid>();
                if (task.CreatedBy.HasValue) recipients.Add(task.CreatedBy.Value);
                if (task.AssigneeId.HasValue) recipients.Add(task.AssigneeId.Value);
                foreach (var uid in parsedAssigneeIds) recipients.Add(uid);

                if (recipients.Count > 0)
                {
                    Guid? convId = null;
                    if (task.SourceMessage != null) convId = task.SourceMessage.ConversationId;
                    else if (!string.IsNullOrEmpty(task.RefId) && Guid.TryParse(task.RefId, out var g)) convId = g;

                    var reminderDto = new TaskReminderDto
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        ConversationId = convId,
                        Type = reminderType,
                        Deadline = deadlineUtc,
                        Message = message
                    };

                    await chatNotificationService.SendTaskReminderAsync(reminderDto, recipients.ToList());
                    _logger.LogInformation("Dispatched task reminder [{Type}] for task {TaskId} ({TaskTitle})", reminderType, task.Id, task.Title);
                }

                // Update task description with reminder tag to prevent duplicates
                task.Description = TaskMetadataHelper.AddReminderTag(task.Description, reminderTagToAdd);
                taskRepository.Update(task);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
