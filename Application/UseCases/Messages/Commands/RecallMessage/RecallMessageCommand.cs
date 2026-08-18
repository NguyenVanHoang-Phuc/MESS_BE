using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.RecallMessage;

public record RecallMessageCommand(Guid MessageId, Guid RequesterId) : IRequest<Result<MessageResponse>>;
