using MediatR;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Auth.Commands.SendRegistrationOtp;

public class SendRegistrationOtpCommand : IRequest<Result<SendOtpResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SendOtpResponse
{
    public string Email { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; } = 300;
    public string Message { get; set; } = string.Empty;
}
