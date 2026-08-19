using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(
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

    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var assigneesList = request.AssigneeIds ?? new List<Guid>();
        if (request.AssigneeId.HasValue && !assigneesList.Contains(request.AssigneeId.Value))
        {
            assigneesList.Insert(0, request.AssigneeId.Value);
        }

        var primaryAssigneeId = assigneesList.FirstOrDefault();
        var priority = request.Priority ?? "Medium";
        var encodedDescription = TaskMetadataHelper.FormatDescriptionWithAssignees(request.Description, assigneesList);

        Guid? sourceMessageId = request.SourceMessageId;
        MESS.Application.DTOs.Responses.Messages.MessageResponse? createdMessageResponse = null;

        // If manual task creation (from input toolbar), create an announcement message in the chat
        if (!sourceMessageId.HasValue && request.ConversationId.HasValue)
        {
            var announcementMsg = new MESS.Domain.Entities.Message
            {
                Id = Guid.NewGuid(),
                ConversationId = request.ConversationId.Value,
                SenderId = request.CreatorId,
                Content = $"📋 Đã tạo công việc mới: {request.Title}",
                CreatedBy = request.CreatorId,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(announcementMsg);
            sourceMessageId = announcementMsg.Id;
        }

        var task = new MESS.Domain.Entities.Task
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = encodedDescription,
            AssigneeId = primaryAssigneeId != Guid.Empty ? primaryAssigneeId : null,
            SourceMessageId = sourceMessageId,
            RefId = request.ConversationId?.ToString(),
            Deadline = request.Deadline,
            Status = "Todo",
            RefType = priority,
            CreatedBy = request.CreatorId,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // If an announcement message was created, fetch and map it for realtime broadcast
        if (sourceMessageId.HasValue && !request.SourceMessageId.HasValue)
        {
            var fullMsg = await _messageRepository.GetByIdWithDetailsAsync(sourceMessageId.Value);
            if (fullMsg != null)
            {
                createdMessageResponse = _mapper.Map<MESS.Application.DTOs.Responses.Messages.MessageResponse>(fullMsg);
            }
        }

        var fullTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        var response = _mapper.Map<TaskResponse>(fullTask);

        response.Description = TaskMetadataHelper.ParseDescription(fullTask?.Description).CleanDescription;
        response.ConversationId = request.ConversationId ?? response.ConversationId;
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
        var notifyUsers = new HashSet<Guid> { request.CreatorId };
        foreach (var uid in assigneesList)
        {
            notifyUsers.Add(uid);
        }

        if (request.ConversationId.HasValue)
        {
            var participants = await _participantRepository.GetConversationParticipantsAsync(request.ConversationId.Value);
            foreach (var p in participants)
            {
                notifyUsers.Add(p.UserId);
            }
        }

        if (request.SourceMessageId.HasValue)
        {
            var message = await _messageRepository.GetByIdWithDetailsAsync(request.SourceMessageId.Value);
            if (message != null)
            {
                var participants = await _participantRepository.GetConversationParticipantsAsync(message.ConversationId);
                foreach (var p in participants)
                {
                    notifyUsers.Add(p.UserId);
                }
            }
        }

        // If an announcement message was generated, broadcast it to all participants in conversation
        if (createdMessageResponse != null && request.ConversationId.HasValue)
        {
            var participants = await _participantRepository.GetConversationParticipantsAsync(request.ConversationId.Value);
            var participantIds = participants.Select(p => p.UserId).ToList();
            await _chatNotificationService.SendNewMessageAsync(createdMessageResponse, participantIds);
        }

        await _chatNotificationService.SendNewTaskAsync(response, notifyUsers.ToList());

        return Result<TaskResponse>.Success(response);
    }
}
