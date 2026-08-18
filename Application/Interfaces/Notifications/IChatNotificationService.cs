using MESS.Application.DTOs.Responses.Messages;

namespace MESS.Application.Interfaces.Notifications;

public interface IChatNotificationService
{
    Task SendNewMessageAsync(MessageResponse message, List<Guid> participantIds);
}
