using BasePlatform.Api.Common;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.ConfirmEmail;
using BasePlatform.Application.Features.Auth.ForgotPassword;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Application.Features.Auth.Logout;
using BasePlatform.Application.Features.Auth.RefreshToken;
using BasePlatform.Application.Features.Auth.Register;
using BasePlatform.Application.Features.Auth.ResendConfirmation;
using BasePlatform.Application.Features.Auth.PhoneLogin;
using BasePlatform.Application.Features.Auth.PhoneRegister;
using BasePlatform.Application.Features.Auth.ResetPassword;
using BasePlatform.Application.Features.Auth.VerifyResetCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AuthController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? NoContent() : Problem(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // POST api/auth/confirm-email  { email, code }
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Email confirmed successfully." })
            : Problem(result);
    }

    // POST api/auth/resend-confirmation  { email }
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "If an account requires confirmation, a new code has been sent." })
            : Problem(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "If your email exists, a password reset code has been sent." })
            : Problem(result);
    }

    // POST api/auth/verify-reset-code  { email, code }
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode(
        [FromBody] VerifyResetCodeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Reset code verified." })
            : Problem(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Password reset successfully." })
            : Problem(result);
    }

    // POST api/auth/phone/register/send-code  { phoneNumber, firstName, lastName }
    [HttpPost("phone/register/send-code")]
    public async Task<IActionResult> SendPhoneRegisterCode(
        [FromBody] SendPhoneRegisterCodeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // POST api/auth/phone/register/verify  { phoneNumber, code }
    [HttpPost("phone/register/verify")]
    public async Task<IActionResult> VerifyPhoneRegister(
        [FromBody] VerifyPhoneRegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // POST api/auth/phone/login/send-code  { phoneNumber }
    [HttpPost("phone/login/send-code")]
    public async Task<IActionResult> SendPhoneLoginCode(
        [FromBody] SendPhoneLoginCodeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // POST api/auth/phone/login/verify  { phoneNumber, code }
    [HttpPost("phone/login/verify")]
    public async Task<IActionResult> VerifyPhoneLogin(
        [FromBody] VerifyPhoneLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }
}
