using MediatR;
using MESS.Application.DTOs.Responses.Auth;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
