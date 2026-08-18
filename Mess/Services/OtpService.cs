using System.Security.Cryptography;
using MESS.Application.Interfaces.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OtpService> _logger;

    public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public string GenerateOtpCode()
    {
        // Generate secure 6-digit random number
        var number = RandomNumberGenerator.GetInt32(100000, 1000000);
        return number.ToString("D6");
    }

    public void StoreRegistrationOtp(string email, string fullName, string passwordHash, string otpCode, int expirationMinutes = 5)
    {
        var key = GetCacheKey(email);
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var data = new OtpRegistrationData
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            OtpCode = otpCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _cache.Set(key, data, TimeSpan.FromMinutes(expirationMinutes));
        _logger.LogInformation("Stored registration OTP for {Email} (Expires at {ExpiresAt})", email, expiresAt);
    }

    public (bool isValid, string? errorMessage, OtpRegistrationData? data) ValidateAndConsumeOtp(string email, string otpCode)
    {
        var key = GetCacheKey(email);
        if (!_cache.TryGetValue<OtpRegistrationData>(key, out var data) || data == null)
        {
            return (false, "Mã xác thực OTP không tồn tại hoặc đã hết hạn.", null);
        }

        if (DateTime.UtcNow > data.ExpiresAt)
        {
            _cache.Remove(key);
            return (false, "Mã xác thực OTP đã hết hạn. Vui lòng lấy mã mới.", null);
        }

        if (!string.Equals(data.OtpCode.Trim(), otpCode.Trim(), StringComparison.Ordinal))
        {
            return (false, "Mã xác thực OTP không chính xác.", null);
        }

        // Consume OTP
        _cache.Remove(key);
        return (true, null, data);
    }

    private static string GetCacheKey(string email) => $"otp:register:{email.Trim().ToLowerInvariant()}";
}
