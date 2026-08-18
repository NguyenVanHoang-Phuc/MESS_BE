using System.Threading.Tasks;

namespace MESS.Application.Interfaces.Email;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode, int expirationMinutes = 5);
}
