using MESS.Application.DTOs.Responses.Conversations;

namespace MESS.Application.DTOs.Responses.Messages;

public class SendDirectMessageResponse
{
    public ConversationResponse Conversation { get; set; } = null!;
    public MessageResponse Message { get; set; } = null!;
    public bool WasConversationCreated { get; set; }
}
