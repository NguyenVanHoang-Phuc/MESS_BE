namespace MESS.Application.DTOs.Responses.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? DepartmentName { get; set; }
    public string? RoleName { get; set; }
}
