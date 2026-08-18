using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.SendDirectMessage;

public class SendDirectMessageCommandHandler : IRequestHandler<SendDirectMessageCommand, Result<SendDirectMessageResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SendDirectMessageCommandHandler(
        IUserRepository userRepository,
        IConversationRepository conversationRepository,
        IParticipantRepository participantRepository,
        IMessageRepository messageRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _messageRepository = messageRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SendDirectMessageResponse>> Handle(
        SendDirectMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate self-messaging
        if (request.SenderId == request.RecipientId)
        {
            return Result<SendDirectMessageResponse>.Failure(DomainErrors.Conversation.CannotCreateDirectWithSelf);
        }

        // 2. Validate empty content and empty attachments
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var hasAttachments = request.Attachments != null && request.Attachments.Count > 0;
        if (!hasContent && !hasAttachments)
        {
            return Result<SendDirectMessageResponse>.Failure(DomainErrors.Message.Empty);
        }

        // 3. Validate recipient exists & active
        var recipient = await _userRepository.GetByIdWithDetailsAsync(request.RecipientId);
        if (recipient == null || !recipient.IsActive)
        {
            return Result<SendDirectMessageResponse>.Failure(DomainErrors.User.NotFound);
        }

        // 4. Find or create direct conversation
        var conversation = await _conversationRepository.FindDirectConversationAsync(request.SenderId, request.RecipientId);
        bool wasCreated = false;

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = "Direct",
                Title = null,
                CreatedBy = request.SenderId,
                CreatedAt = DateTime.UtcNow
            };
            await _conversationRepository.AddAsync(conversation);

            var participant1 = new Participant
            {
                ConversationId = conversation.Id,
                UserId = request.SenderId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            var participant2 = new Participant
            {
                ConversationId = conversation.Id,
                UserId = request.RecipientId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            await _participantRepository.AddAsync(participant1);
            await _participantRepository.AddAsync(participant2);
            wasCreated = true;
        }

        // 5. Create Message
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.SenderId,
            Content = request.Content,
            CreatedBy = request.SenderId,
            CreatedAt = DateTime.UtcNow
        };

        if (hasAttachments)
        {
            foreach (var att in request.Attachments!)
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

        // 6. Map responses
        var fullMessage = await _messageRepository.GetByIdWithDetailsAsync(message.Id);
        var messageResponse = _mapper.Map<MessageResponse>(fullMessage);

        var fullConversation = await _conversationRepository.GetByIdWithDetailsAsync(conversation.Id);
        var conversationResponse = _mapper.Map<ConversationResponse>(fullConversation);

        var targetParticipantIds = new List<Guid> { request.SenderId, request.RecipientId };

        // 7. Broadcast realtime notifications
        if (wasCreated)
        {
            await _chatNotificationService.SendNewConversationAsync(conversationResponse, targetParticipantIds);
        }

        await _chatNotificationService.SendNewMessageAsync(messageResponse, targetParticipantIds);

        return Result<SendDirectMessageResponse>.Success(new SendDirectMessageResponse
        {
            Conversation = conversationResponse,
            Message = messageResponse,
            WasConversationCreated = wasCreated
        });
    }
}
