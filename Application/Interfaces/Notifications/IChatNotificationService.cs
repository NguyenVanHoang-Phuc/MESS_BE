using MESS.Application.DTOs.Responses.Conversations;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.DTOs.Responses.Tasks;

namespace MESS.Application.Interfaces.Notifications;

public interface IChatNotificationService
{
    Task SendNewMessageAsync(MessageResponse message, List<Guid> participantIds);
    Task SendNewConversationAsync(ConversationResponse conversation, List<Guid> participantIds);
    Task SendConversationDeletedAsync(Guid conversationId, List<Guid> participantIds);
    Task SendMessagesReadAsync(Guid conversationId, Guid readerId, string readerName, List<Guid> messageIds, List<Guid> participantIds);
    Task SendMessageRecalledAsync(Guid conversationId, Guid messageId, List<Guid> participantIds);
    Task SendMessageReactionAsync(Guid conversationId, Guid messageId, List<ReactionResponse> reactions, List<Guid> participantIds);
    Task SendNewTaskAsync(TaskResponse task, List<Guid> participantIds);
    Task SendTaskUpdatedAsync(TaskResponse task, List<Guid> participantIds);
    Task SendTaskDeletedAsync(Guid taskId, Guid? conversationId, List<Guid> participantIds);
    Task SendTaskReminderAsync(TaskReminderDto reminder, List<Guid> participantIds);
}
