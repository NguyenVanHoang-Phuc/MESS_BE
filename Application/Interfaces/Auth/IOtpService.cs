namespace MESS.Application.Interfaces.Auth;

public class OtpRegistrationData
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}

public interface IOtpService
{
    string GenerateOtpCode();
    void StoreRegistrationOtp(string email, string fullName, string passwordHash, string otpCode, int expirationMinutes = 5);
    (bool isValid, string? errorMessage, OtpRegistrationData? data) ValidateAndConsumeOtp(string email, string otpCode);
}
