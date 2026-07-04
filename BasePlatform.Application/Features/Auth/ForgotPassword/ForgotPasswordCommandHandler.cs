using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly OtpCodeIssuer _otpIssuer;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;

    public ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager,
        OtpCodeIssuer otpIssuer,
        IEmailVerificationCodeRepository codeRepository,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy)
    {
        _userManager = userManager;
        _otpIssuer = otpIssuer;
        _codeRepository = codeRepository;
        _clock = clock;
        _policy = policy;
    }

    public async Task<Result> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        // Anti-enumeration: always succeed. Initial send and resend both hit this endpoint.
        var user = await _userManager.FindByEmailAsync(command.Email.Trim());

        if (user is not null && user.IsActive
            && await OtpRateLimitHelper.IsWithinLimitsAsync(
                _codeRepository, _clock, _policy, user.Id, OtpPurpose.PasswordReset, cancellationToken))
        {
            await _otpIssuer.IssueAsync(user, OtpPurpose.PasswordReset, cancellationToken);
        }

        return Result.Success();
    }
}
