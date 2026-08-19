using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, Result<List<TaskResponse>>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(ITaskRepository taskRepository, IUserRepository userRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<TaskResponse>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetTasksByFilterAsync(
            request.ConversationId,
            request.MessageId,
            request.AssigneeId,
            request.CreatorId,
            request.Status);

        var response = _mapper.Map<List<TaskResponse>>(tasks);

        // Populate clean description and multiple assignees
        for (int i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            var r = response[i];

            var (cleanDesc, parsedAssigneeIds) = TaskMetadataHelper.ParseDescription(t.Description);
            r.Description = cleanDesc;

            var uids = new List<Guid>(parsedAssigneeIds);

            // Backward compatibility for old RefType encoding
            if (uids.Count == 0 && !string.IsNullOrEmpty(t.RefType) && t.RefType.Contains('#'))
            {
                var parts = t.RefType.Split('#');
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

            if (uids.Count == 0 && t.AssigneeId.HasValue)
            {
                uids.Add(t.AssigneeId.Value);
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
                r.AssigneeIds = uids;
                r.Assignees = userList;
                if (userList.Count > 0)
                {
                    r.AssigneeName = string.Join(", ", userList.Select(u => u.FullName));
                }
            }
        }

        return Result<List<TaskResponse>>.Success(response);
    }
}
