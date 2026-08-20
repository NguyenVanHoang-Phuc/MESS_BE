using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private static readonly object _lock = new();

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(userId))
        {
            bool isFirstConnection = false;
            lock (_lock)
            {
                var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
                isFirstConnection = connections.Count == 0;
                connections.Add(Context.ConnectionId);
            }

            _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId}).", userId, Context.ConnectionId);

            // Send list of currently online user IDs to the newly connected caller
            var onlineUsers = _userConnections.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToList();
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", onlineUsers);

            // If user just came online, broadcast to other connected clients
            if (isFirstConnection)
            {
                await Clients.Others.SendAsync("UserStatusChanged", userId, true);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(userId))
        {
            bool isLastConnection = false;
            lock (_lock)
            {
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        isLastConnection = true;
                    }
                }
            }

            _logger.LogInformation("User {UserId} disconnected from ChatHub (ConnectionId: {ConnectionId}).", userId, Context.ConnectionId);

            if (isLastConnection)
            {
                await Clients.Others.SendAsync("UserStatusChanged", userId, false);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task<List<string>> GetOnlineUsers()
    {
        var onlineUsers = _userConnections.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToList();
        return Task.FromResult(onlineUsers);
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
        var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("ReceiveUserTyping", new
        {
            ConversationId = conversationId,
            UserId = userId,
            UserName = userName,
            IsTyping = isTyping
        });
    }
}
