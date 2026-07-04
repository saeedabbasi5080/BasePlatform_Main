using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : ICommandHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;

    public ResetPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IEmailVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IRefreshTokenService refreshTokenService,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy)
    {
        _userManager = userManager;
        _codeRepository = codeRepository;
        _codeService = codeService;
        _refreshTokenService = refreshTokenService;
        _clock = clock;
        _policy = policy;
    }

    public async Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null || !user.IsActive)
            return Result.Failure(OtpVerificationHelper.InvalidCodeError);

        var verifyResult = await OtpVerificationHelper.VerifyAsync(
            _codeRepository,
            _codeService,
            _clock,
            _policy,
            user.Id,
            command.Token,
            OtpPurpose.PasswordReset,
            consumeOnSuccess: true,
            cancellationToken);

        if (verifyResult.IsFailure)
            return verifyResult;

        // Apply the new password through Identity so the same policy rules as register apply.
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errorMessage = string.Join("; ", resetResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Auth.PasswordPolicyFailed", errorMessage));
        }

        // OTP delivered to the inbox proves email ownership.
        if (!user.EmailConfirmed)
            user.EmailConfirmed = true;

        user.UpdatedAt = _clock.UtcNow;
        await _userManager.UpdateAsync(user);

        // Invalidate all existing sessions after a password reset.
        await _refreshTokenService.RevokeAllAsync(user.Id, cancellationToken);

        return Result.Success();
    }
}
