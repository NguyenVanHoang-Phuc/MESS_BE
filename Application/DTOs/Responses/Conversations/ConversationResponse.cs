namespace MESS.Application.DTOs.Responses.Conversations;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ParticipantResponse> Participants { get; set; } = new();
    public MessageSummaryResponse? LastMessage { get; set; }
}

public class ParticipantResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class MessageSummaryResponse
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
