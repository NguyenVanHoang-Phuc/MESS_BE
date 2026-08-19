using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to ChatHub with ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task SendTyping(string conversationId, string userName, bool isTyping)
    {
        var userId = Context.UserIdentifier;
        await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("ReceiveUserTyping", new
        {
            ConversationId = conversationId,
            UserId = userId,
            UserName = userName,
            IsTyping = isTyping
        });
    }
}
