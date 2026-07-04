using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.ResendConfirmation;

public sealed class ResendConfirmationCommandHandler : ICommandHandler<ResendConfirmationCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly OtpCodeIssuer _otpIssuer;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;

    public ResendConfirmationCommandHandler(
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
        ResendConfirmationCommand command,
        CancellationToken cancellationToken = default)
    {
        // Anti-enumeration: every call returns success. Throttling is enforced silently.
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is not null && user.IsActive && !user.EmailConfirmed
            && await OtpRateLimitHelper.IsWithinLimitsAsync(
                _codeRepository, _clock, _policy, user.Id, OtpPurpose.EmailConfirmation, cancellationToken))
        {
            await _otpIssuer.IssueAsync(user, OtpPurpose.EmailConfirmation, cancellationToken);
        }

        return Result.Success();
    }
}
