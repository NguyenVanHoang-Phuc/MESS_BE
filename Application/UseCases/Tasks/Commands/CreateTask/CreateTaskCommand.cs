using System;
using System.Collections.Generic;
using MediatR;
using MESS.Application.DTOs.Responses.Tasks;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Tasks.Commands.CreateTask;

public class CreateTaskCommand : IRequest<Result<TaskResponse>>
{
    public Guid CreatorId { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid? SourceMessageId { get; set; }
    public Guid? AssigneeId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Priority { get; set; }
}
