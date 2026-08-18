namespace MESS.Application.DTOs.Responses.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? RoleName { get; set; }
    public string? DepartmentName { get; set; }
}
