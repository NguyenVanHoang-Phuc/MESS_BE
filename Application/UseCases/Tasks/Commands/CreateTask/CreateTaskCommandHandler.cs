using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new MESS.Domain.Entities.Task
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            SourceMessageId = request.SourceMessageId,
            Deadline = request.Deadline,
            Status = "Todo",
            CreatedBy = request.CreatorId,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var fullTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        var response = _mapper.Map<TaskResponse>(fullTask);
        return Result<TaskResponse>.Success(response);
    }
}
