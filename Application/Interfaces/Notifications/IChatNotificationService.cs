using MESS.Application.DTOs.Responses.Conversations;
using MESS.Application.DTOs.Responses.Messages;

namespace MESS.Application.Interfaces.Notifications;

public interface IChatNotificationService
{
    Task SendNewMessageAsync(MessageResponse message, List<Guid> participantIds);
    Task SendNewConversationAsync(ConversationResponse conversation, List<Guid> participantIds);
    Task SendConversationDeletedAsync(Guid conversationId, List<Guid> participantIds);
    Task SendMessagesReadAsync(Guid conversationId, Guid readerId, string readerName, List<Guid> messageIds, List<Guid> participantIds);
}
