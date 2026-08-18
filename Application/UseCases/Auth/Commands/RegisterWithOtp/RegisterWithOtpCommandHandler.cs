using MediatR;
using MESS.Application.DTOs.Responses.Auth;
using MESS.Application.Interfaces.Auth;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace MESS.Application.UseCases.Auth.Commands.RegisterWithOtp;

public class RegisterWithOtpCommandHandler : IRequestHandler<RegisterWithOtpCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterWithOtpCommandHandler> _logger;

    public RegisterWithOtpCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterWithOtpCommandHandler> logger)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(RegisterWithOtpCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var otpCode = request.OtpCode?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
        {
            return Result<LoginResponse>.Failure(new Error("Auth.MissingData", "Vui lòng cung cấp email và mã OTP."));
        }

        var (isValid, errorMessage, registrationData) = _otpService.ValidateAndConsumeOtp(email, otpCode);
        if (!isValid || registrationData == null)
        {
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidOtp", errorMessage ?? "Mã xác thực OTP không chính xác."));
        }

        // Final check if user already exists
        var existing = await _userRepository.FindByUsernameAsync(registrationData.Email);
        if (existing != null)
        {
            return Result<LoginResponse>.Failure(DomainErrors.User.EmailAlreadyExists);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = registrationData.Email,
            FullName = registrationData.FullName,
            PasswordHash = registrationData.PasswordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully registered user {Email} with ID {UserId}", newUser.Username, newUser.Id);

        var token = _tokenService.GenerateToken(newUser, Array.Empty<string>());

        var response = new LoginResponse
        {
            AccessToken = token,
            UserId = newUser.Id,
            Username = newUser.Username,
            FullName = newUser.FullName,
            RoleName = "Member",
            DepartmentName = null
        };

        return Result<LoginResponse>.Success(response);
    }
}
