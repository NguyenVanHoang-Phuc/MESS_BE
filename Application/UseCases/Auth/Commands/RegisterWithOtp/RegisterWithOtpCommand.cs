using MediatR;
using MESS.Application.DTOs.Responses.Auth;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Auth.Commands.RegisterWithOtp;

public class RegisterWithOtpCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
