using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.RecallMessage;

public class RecallMessageCommandHandler : IRequestHandler<RecallMessageCommand, Result<MessageResponse>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecallMessageCommandHandler(
        IMessageRepository messageRepository,
        IParticipantRepository participantRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MessageResponse>> Handle(RecallMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdWithDetailsAsync(request.MessageId);
        if (message == null)
            return Result<MessageResponse>.Failure(DomainErrors.Message.NotFound);

        if (message.IsDeleted)
            return Result<MessageResponse>.Failure(DomainErrors.Message.AlreadyDeleted);

        if (message.IsRecalled)
            return Result<MessageResponse>.Failure(DomainErrors.Message.AlreadyRecalled);

        if (message.SenderId != request.RequesterId)
            return Result<MessageResponse>.Failure(DomainErrors.Message.AccessDenied);

        // Business rule: Check recall time limit (within 24 hours)
        if ((DateTime.UtcNow - message.CreatedAt).TotalHours > 24)
            return Result<MessageResponse>.Failure(DomainErrors.Message.RecallTimeExpired);

        message.IsRecalled = true;
        message.Content = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<MessageResponse>(message);

        var participants = await _participantRepository.GetConversationParticipantsAsync(message.ConversationId);
        var participantIds = participants.Select(p => p.UserId).ToList();

        await _chatNotificationService.SendMessageRecalledAsync(message.ConversationId, message.Id, participantIds);

        return Result<MessageResponse>.Success(response);
    }
}
