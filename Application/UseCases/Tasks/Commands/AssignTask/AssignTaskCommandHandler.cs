using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.AssignTask;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AssignTaskCommandHandler(
        ITaskRepository taskRepository,
        IMessageRepository messageRepository,
        IParticipantRepository participantRepository,
        IUserRepository userRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _userRepository = userRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TaskResponse>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(request.TaskId);
        if (task == null)
            return Result<TaskResponse>.Failure(new Error("Task.NotFound", "Không tìm thấy công việc này."));

        var assigneesList = request.AssigneeIds ?? (request.AssigneeId.HasValue ? new List<Guid> { request.AssigneeId.Value } : new List<Guid>());
        var primaryAssigneeId = assigneesList.FirstOrDefault();

        var currentPriority = !string.IsNullOrEmpty(task.RefType) ? task.RefType.Split('#')[0] : "Medium";
        var cleanDesc = TaskMetadataHelper.ParseDescription(task.Description).CleanDescription;
        var encodedDescription = TaskMetadataHelper.FormatDescriptionWithAssignees(cleanDesc, assigneesList);

        // Update assignee and description safely
        task.AssigneeId = primaryAssigneeId != Guid.Empty ? primaryAssigneeId : null;
        task.RefType = currentPriority;
        task.Description = encodedDescription;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var fullTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        var response = _mapper.Map<TaskResponse>(fullTask);

        response.Description = cleanDesc;

        response.AssigneeIds = assigneesList;
        if (assigneesList.Count > 0)
        {
            var userList = new List<TaskAssigneeDto>();
            foreach (var uid in assigneesList)
            {
                var u = await _userRepository.GetByIdAsync(uid);
                if (u != null)
                {
                    userList.Add(new TaskAssigneeDto { UserId = u.Id, FullName = u.FullName });
                }
            }
            response.Assignees = userList;
            if (userList.Count > 0)
            {
                response.AssigneeName = string.Join(", ", userList.Select(u => u.FullName));
            }
        }

        // Notify participants via SignalR
        var notifyUsers = new HashSet<Guid>();
        if (task.CreatedBy.HasValue) notifyUsers.Add(task.CreatedBy.Value);
        foreach (var uid in assigneesList)
        {
            notifyUsers.Add(uid);
        }

        if (!string.IsNullOrEmpty(task.RefId) && Guid.TryParse(task.RefId, out var convGuid))
        {
            var participants = await _participantRepository.GetConversationParticipantsAsync(convGuid);
            foreach (var p in participants) notifyUsers.Add(p.UserId);
        }

        if (task.SourceMessageId.HasValue)
        {
            var message = await _messageRepository.GetByIdWithDetailsAsync(task.SourceMessageId.Value);
            if (message != null)
            {
                var participants = await _participantRepository.GetConversationParticipantsAsync(message.ConversationId);
                foreach (var p in participants)
                {
                    notifyUsers.Add(p.UserId);
                }
            }
        }

        await _chatNotificationService.SendTaskUpdatedAsync(response, notifyUsers.ToList());

        return Result<TaskResponse>.Success(response);
    }
}
