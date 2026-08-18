using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace MESS.Mess.Providers;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Try to get UserId from "sub" or "NameIdentifier" claim
        var userId = connection.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                  ?? connection.User?.FindFirstValue("sub");
                  
        return userId;
    }
}
