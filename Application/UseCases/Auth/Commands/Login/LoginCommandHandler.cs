using MediatR;
using MESS.Application.DTOs.Responses.Auth;
using MESS.Application.Interfaces.Auth;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace MESS.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        var user = await _userRepository.FindByUsernameAsync(request.Username);
        if (user is null)
        {
            _logger.LogWarning("Login failed: User not found. Username: {Username}", request.Username);
            return Result<LoginResponse>.Failure(DomainErrors.User.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User inactive. UserId: {UserId}", user.Id);
            return Result<LoginResponse>.Failure(DomainErrors.User.Inactive);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash ?? string.Empty))
        {
            _logger.LogWarning("Login failed: Wrong password. UserId: {UserId}", user.Id);
            return Result<LoginResponse>.Failure(DomainErrors.User.InvalidCredentials);
        }

        var roles = user.Role is not null ? new[] { user.Role.Name } : Array.Empty<string>();
        var token = _tokenService.GenerateToken(user, roles);

        var response = new LoginResponse
        {
            AccessToken = token,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = user.Role?.Name,
            DepartmentName = user.Department?.Name
        };

        _logger.LogInformation("Login successful. UserId: {UserId}", user.Id);
        return Result<LoginResponse>.Success(response);
    }
}
