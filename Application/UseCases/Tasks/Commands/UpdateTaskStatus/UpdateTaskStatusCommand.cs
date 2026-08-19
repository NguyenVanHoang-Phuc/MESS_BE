using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.UpdateTaskStatus;

public class UpdateTaskStatusCommand : IRequest<Result<TaskResponse>>
{
    public Guid TaskId { get; set; }
    public string Status { get; set; } = "Todo"; // Todo, InProgress, Done
    public Guid CurrentUserId { get; set; }
}
