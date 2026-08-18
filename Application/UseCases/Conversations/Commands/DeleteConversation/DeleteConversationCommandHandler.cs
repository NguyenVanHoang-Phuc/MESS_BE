using MediatR;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.DeleteConversation;

public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, Result>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConversationCommandHandler(
        IConversationRepository conversationRepository,
        IParticipantRepository participantRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(request.ConversationId);
        if (conversation is null)
            return Result.Failure(DomainErrors.Conversation.NotFoundById(request.ConversationId));

        if (conversation.Type != "Group")
            return Result.Failure(DomainErrors.Conversation.NotGroup);

        // Kiểm tra quyền: Người yêu cầu phải là Admin hoặc Creator của nhóm
        var requesterParticipant = await _participantRepository.FindAsync(request.ConversationId, request.RequesterId);
        var isCreator = conversation.CreatedBy == request.RequesterId;
        var isAdmin = requesterParticipant?.Role == "Admin";

        if (!isCreator && !isAdmin)
            return Result.Failure(DomainErrors.Conversation.NotAdmin);

        // Lấy danh sách thành viên trước khi xóa để bắn SignalR
        var participants = await _participantRepository.GetConversationParticipantsAsync(request.ConversationId);
        var participantIds = participants.Select(p => p.UserId).ToList();

        // Xóa cuộc hội thoại (cascade xóa toàn bộ tin nhắn, file đính kèm, reactions, thành viên)
        _conversationRepository.Remove(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Bắn SignalR thông báo giải tán nhóm cho tất cả thành viên
        await _chatNotificationService.SendConversationDeletedAsync(request.ConversationId, participantIds);

        return Result.Success();
    }
}
