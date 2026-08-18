using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageResponse>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IParticipantRepository participantRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra participant
        var isParticipant = await _conversationRepository.IsParticipantAsync(request.ConversationId, request.SenderId);
        if (!isParticipant)
            return Result<MessageResponse>.Failure(DomainErrors.Conversation.AccessDenied);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            CreatedBy = request.SenderId,
            CreatedAt = DateTime.UtcNow
        };

        // Attachments
        if (request.Attachments != null && request.Attachments.Count > 0)
        {
            foreach (var att in request.Attachments)
            {
                var safeFileType = att.FileType;
                if (!string.IsNullOrEmpty(safeFileType) && safeFileType.Length > 50)
                {
                    if (safeFileType.Contains("spreadsheetml")) safeFileType = "application/vnd.ms-excel";
                    else if (safeFileType.Contains("wordprocessingml")) safeFileType = "application/msword";
                    else if (safeFileType.Contains("presentationml")) safeFileType = "application/vnd.ms-powerpoint";
                    else safeFileType = safeFileType.Substring(0, 50);
                }

                message.Attachments.Add(new Attachment
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    FileUrl = att.FileUrl,
                    FileType = safeFileType,
                    FileSize = att.FileSize,
                    CreatedBy = request.SenderId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var fullMessage = await _messageRepository.GetByIdWithDetailsAsync(message.Id);
        var response = _mapper.Map<MessageResponse>(fullMessage);

        // Fetch participants and send realtime notification
        var participants = await _participantRepository.GetConversationParticipantsAsync(request.ConversationId);
        var participantIds = participants.Select(p => p.UserId).ToList();

        await _chatNotificationService.SendNewMessageAsync(response, participantIds);

        return Result<MessageResponse>.Success(response);
    }
}
