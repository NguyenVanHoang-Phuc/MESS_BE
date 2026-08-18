namespace MESS.Application.DTOs.Requests.Conversations;

public class CreateConversationRequest
{
    public string Type { get; set; } = "Direct"; // Direct | Group
    public string? Title { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}
