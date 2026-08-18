namespace MESS.Application.DTOs.Responses.Tasks;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public Guid? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public Guid? SourceMessageId { get; set; }
    public DateTime CreatedAt { get; set; }
}
