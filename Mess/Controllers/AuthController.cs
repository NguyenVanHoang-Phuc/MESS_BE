using MESS.Application.UseCases.Auth.Commands.Login;
using MESS.Application.UseCases.Auth.Commands.RegisterWithOtp;
using MESS.Application.UseCases.Auth.Commands.SendRegistrationOtp;
using MESS.Mess.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("register/send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendRegistrationOtp([FromBody] SendRegistrationOtpCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("register/verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] RegisterWithOtpCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
