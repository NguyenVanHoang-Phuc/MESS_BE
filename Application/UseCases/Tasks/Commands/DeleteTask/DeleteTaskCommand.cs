using System;
using MediatR;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.DeleteTask;

public class DeleteTaskCommand : IRequest<Result>
{
    public Guid TaskId { get; set; }
    public Guid CurrentUserId { get; set; }
}
