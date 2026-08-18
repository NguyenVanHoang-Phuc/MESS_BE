using MediatR;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.MarkConversationAsRead;

public class MarkConversationAsReadCommandHandler : IRequestHandler<MarkConversationAsReadCommand, Result>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkConversationAsReadCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IParticipantRepository participantRepository,
        IUserRepository userRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _userRepository = userRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkConversationAsReadCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền truy cập
        var isParticipant = await _conversationRepository.IsParticipantAsync(request.ConversationId, request.ReaderId);
        if (!isParticipant)
            return Result.Failure(DomainErrors.Conversation.AccessDenied);

        // Lấy danh sách tin nhắn chưa đọc trong hội thoại
        var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(request.ConversationId, request.ReaderId);
        if (unreadMessages == null || unreadMessages.Count == 0)
            return Result.Success();

        var messageReads = unreadMessages.Select(msg => new MessageRead
        {
            MessageId = msg.Id,
            UserId = request.ReaderId,
            ReadAt = DateTime.UtcNow
        }).ToList();

        await _messageRepository.AddMessageReadsAsync(messageReads);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Phát sự kiện SignalR tới toàn bộ thành viên trong nhóm
        var participants = await _participantRepository.GetConversationParticipantsAsync(request.ConversationId);
        var participantIds = participants.Select(p => p.UserId).ToList();
        var reader = await _userRepository.GetByIdWithDetailsAsync(request.ReaderId);

        var messageIds = unreadMessages.Select(m => m.Id).ToList();
        await _chatNotificationService.SendMessagesReadAsync(
            request.ConversationId,
            request.ReaderId,
            reader?.FullName ?? "User",
            messageIds,
            participantIds);

        return Result.Success();
    }
}
