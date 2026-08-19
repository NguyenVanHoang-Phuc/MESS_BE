using System;
using System.Collections.Generic;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.AssignTask;

public class AssignTaskCommand : IRequest<Result<TaskResponse>>
{
    public Guid TaskId { get; set; }
    public Guid? AssigneeId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
    public Guid CurrentUserId { get; set; }
}
