using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.ReactMessage;

public record ReactMessageCommand(Guid MessageId, Guid UserId, string Emoji) : IRequest<Result<List<ReactionResponse>>>;
