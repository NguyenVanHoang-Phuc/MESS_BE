using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(
        ITaskRepository taskRepository,
        IMessageRepository messageRepository,
        IParticipantRepository participantRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(request.TaskId);
        if (task == null)
            return Result.Failure(new Error("Task.NotFound", "Không tìm thấy công việc này."));

        Guid? convId = null;
        if (task.SourceMessage != null)
        {
            convId = task.SourceMessage.ConversationId;
        }
        else if (!string.IsNullOrEmpty(task.RefId) && Guid.TryParse(task.RefId, out var parsedConvId))
        {
            convId = parsedConvId;
        }

        var (cleanDesc, parsedAssigneeIds) = TaskMetadataHelper.ParseDescription(task.Description);
        var notifyUsers = new HashSet<Guid>();
        if (task.CreatedBy.HasValue) notifyUsers.Add(task.CreatedBy.Value);
        if (task.AssigneeId.HasValue) notifyUsers.Add(task.AssigneeId.Value);
        foreach (var uid in parsedAssigneeIds) notifyUsers.Add(uid);

        if (convId.HasValue)
        {
            var participants = await _participantRepository.GetConversationParticipantsAsync(convId.Value);
            foreach (var p in participants) notifyUsers.Add(p.UserId);
        }

        // Delete task entity
        _taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify realtime
        await _chatNotificationService.SendTaskDeletedAsync(task.Id, convId, notifyUsers.ToList());

        return Result.Success();
    }
}
