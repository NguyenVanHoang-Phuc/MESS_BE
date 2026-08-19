using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Queries.GetTaskById;

public class GetTaskByIdQuery : IRequest<Result<TaskResponse>>
{
    public Guid Id { get; set; }

    public GetTaskByIdQuery(Guid id)
    {
        Id = id;
    }
}
