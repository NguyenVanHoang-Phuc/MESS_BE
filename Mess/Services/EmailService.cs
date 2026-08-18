using System.Net;
using System.Net.Mail;
using MESS.Application.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MESS.Mess.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode, int expirationMinutes = 5)
    {
        // 1. Always log OTP prominently in console for easy development & testing
        _logger.LogInformation("==================================================");
        _logger.LogInformation("📧 [OTP EMAIL DISPATCH] To: {Email} ({FullName})", toEmail, fullName);
        _logger.LogInformation("🔑 OTP CODE: >>> {OtpCode} <<< (Expires in {Minutes} min)", otpCode, expirationMinutes);
        _logger.LogInformation("==================================================");

        var smtpHost = _configuration["Smtp:Host"];
        var smtpPortStr = _configuration["Smtp:Port"];
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "no-reply@nexus.local";
        var fromName = _configuration["Smtp:FromName"] ?? "Nexus Workspace";

        // If SMTP is not fully configured, we consider the OTP logged to console as success in Dev
        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
        {
            _logger.LogInformation("SMTP host or user is not configured in appsettings.json. OTP is available in server console logs.");
            return;
        }

        try
        {
            int smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"[{otpCode}] Mã xác thực tài khoản Nexus của bạn",
                IsBodyHtml = true,
                Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; }}
        .container {{ max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 16px; padding: 32px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ text-align: center; margin-bottom: 24px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #0284c7; letter-spacing: -0.5px; }}
        .title {{ font-size: 18px; font-weight: 600; color: #0f172a; margin-top: 8px; }}
        .otp-box {{ background: #f0fdf4; border: 2px dashed #22c55e; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0; }}
        .otp-code {{ font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #15803d; font-family: monospace; }}
        .notice {{ font-size: 13px; color: #64748b; line-height: 1.6; text-align: center; }}
        .footer {{ text-align: center; margin-top: 32px; font-size: 12px; color: #94a3b8; border-top: 1px solid #f1f5f9; padding-top: 16px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Nexus Workspace</div>
            <div class='title'>Xác thực địa chỉ Email</div>
        </div>
        <p>Xin chào <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
        <p>Cảm ơn bạn đã đăng ký tài khoản tại Nexus. Vui lòng sử dụng mã OTP dưới đây để hoàn tất đăng ký:</p>
        <div class='otp-box'>
            <div class='otp-code'>{otpCode}</div>
        </div>
        <p class='notice'>Mã xác thực này có hiệu lực trong vòng <strong>{expirationMinutes} phút</strong>.<br>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
        <div class='footer'>
            &copy; {DateTime.UtcNow.Year} Nexus Workspace. All rights reserved.
        </div>
    </div>
</body>
</html>"
            };

            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
            _logger.LogInformation("Successfully sent OTP email to {ToEmail} via SMTP {Host}", toEmail, smtpHost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email via SMTP to {ToEmail}. Fallback to console OTP.", toEmail);
        }
    }
}
