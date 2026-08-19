using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Queries.GetTaskById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(request.Id);
        if (task == null)
            return Result<TaskResponse>.Failure(new Error("Task.NotFound", "Không tìm thấy công việc này."));

        var response = _mapper.Map<TaskResponse>(task);

        var (cleanDesc, parsedAssigneeIds) = TaskMetadataHelper.ParseDescription(task.Description);
        response.Description = cleanDesc;

        var uids = new List<Guid>(parsedAssigneeIds);

        // Backward compatibility for old RefType encoding
        if (uids.Count == 0 && !string.IsNullOrEmpty(task.RefType) && task.RefType.Contains('#'))
        {
            var parts = task.RefType.Split('#');
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                var idStrs = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in idStrs)
                {
                    if (Guid.TryParse(s, out var uid) && !uids.Contains(uid))
                    {
                        uids.Add(uid);
                    }
                }
            }
        }

        if (uids.Count == 0 && task.AssigneeId.HasValue)
        {
            uids.Add(task.AssigneeId.Value);
        }

        if (uids.Count > 0)
        {
            var userList = new List<TaskAssigneeDto>();
            foreach (var uid in uids)
            {
                var u = await _userRepository.GetByIdAsync(uid);
                if (u != null)
                {
                    userList.Add(new TaskAssigneeDto { UserId = u.Id, FullName = u.FullName });
                }
            }
            response.AssigneeIds = uids;
            response.Assignees = userList;
            if (userList.Count > 0)
            {
                response.AssigneeName = string.Join(", ", userList.Select(u => u.FullName));
            }
        }

        return Result<TaskResponse>.Success(response);
    }
}
