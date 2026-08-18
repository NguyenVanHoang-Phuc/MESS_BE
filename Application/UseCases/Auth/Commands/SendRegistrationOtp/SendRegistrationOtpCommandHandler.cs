using MediatR;
using MESS.Application.Interfaces.Auth;
using MESS.Application.Interfaces.Email;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace MESS.Application.UseCases.Auth.Commands.SendRegistrationOtp;

public class SendRegistrationOtpCommandHandler : IRequestHandler<SendRegistrationOtpCommand, Result<SendOtpResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendRegistrationOtpCommandHandler> _logger;

    public SendRegistrationOtpCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<SendRegistrationOtpCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<SendOtpResponse>> Handle(SendRegistrationOtpCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result<SendOtpResponse>.Failure(new Error("Auth.InvalidEmail", "Địa chỉ email không hợp lệ."));
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Result<SendOtpResponse>.Failure(new Error("Auth.InvalidName", "Vui lòng nhập họ và tên của bạn."));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Result<SendOtpResponse>.Failure(new Error("Auth.InvalidPassword", "Mật khẩu phải có độ dài tối thiểu 6 ký tự."));
        }

        // Check if user with this username/email already exists
        var existingUser = await _userRepository.FindByUsernameAsync(email);
        if (existingUser != null)
        {
            return Result<SendOtpResponse>.Failure(DomainErrors.User.EmailAlreadyExists);
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var otpCode = _otpService.GenerateOtpCode();

        _otpService.StoreRegistrationOtp(email, request.FullName, passwordHash, otpCode, expirationMinutes: 5);

        await _emailService.SendOtpEmailAsync(email, request.FullName, otpCode, expirationMinutes: 5);

        _logger.LogInformation("Sent registration OTP to {Email}", email);

        return Result<SendOtpResponse>.Success(new SendOtpResponse
        {
            Email = email,
            ExpiresInSeconds = 300,
            Message = "Mã xác thực OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
        });
    }
}
