using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Interfaces.Notifications;
using MESS.Mess.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatNotificationService> _logger;

    public ChatNotificationService(IHubContext<ChatHub> hubContext, ILogger<ChatNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNewMessageAsync(MessageResponse message, List<Guid> participantIds)
    {
        var userIds = participantIds.Select(id => id.ToString()).ToList();
        
        try
        {
            await _hubContext.Clients.Users(userIds).SendAsync("ReceiveNewMessage", message);
            _logger.LogInformation("Sent message {MessageId} to participants: {ParticipantIds}", message.Id, string.Join(", ", userIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message {MessageId} to participants", message.Id);
        }
    }
}
