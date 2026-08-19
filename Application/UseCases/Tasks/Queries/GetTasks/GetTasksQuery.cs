using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Queries.GetTasks;

public class GetTasksQuery : IRequest<Result<List<TaskResponse>>>
{
    public Guid? ConversationId { get; set; }
    public Guid? MessageId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? CreatorId { get; set; }
    public string? Status { get; set; }
}
