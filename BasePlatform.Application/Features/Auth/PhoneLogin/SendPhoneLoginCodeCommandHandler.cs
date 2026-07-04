using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed class SendPhoneLoginCodeCommandHandler
    : ICommandHandler<SendPhoneLoginCodeCommand, Result<SendPhoneLoginCodeResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly PhoneOtpIssuer _phoneOtpIssuer;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;
    private readonly ISmsOtpPolicy _smsOtpPolicy;

    public SendPhoneLoginCodeCommandHandler(
        UserManager<AppUser> userManager,
        PhoneOtpIssuer phoneOtpIssuer,
        IEmailVerificationCodeRepository codeRepository,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        ISmsOtpPolicy smsOtpPolicy)
    {
        _userManager = userManager;
        _phoneOtpIssuer = phoneOtpIssuer;
        _codeRepository = codeRepository;
        _clock = clock;
        _policy = policy;
        _smsOtpPolicy = smsOtpPolicy;
    }

    public async Task<Result<SendPhoneLoginCodeResponse>> HandleAsync(
        SendPhoneLoginCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!PhoneNumberNormalizer.TryNormalize(command.PhoneNumber, out var phone))
            return Result<SendPhoneLoginCodeResponse>.Failure(
                Error.Validation("Auth.InvalidPhone", "Enter a valid mobile number (09XXXXXXXXX)."));

        if (!_smsOtpPolicy.IsPhoneAllowed(phone))
            return Result<SendPhoneLoginCodeResponse>.Failure(
                Error.Validation(
                    "Auth.PhoneNotAllowed",
                    "SMS is limited to the verified test number (09195200872) until Kavenegar account verification completes."));

        // Anti-enumeration: always return success.
        var user = await PhoneUserLookup.FindByPhoneAsync(_userManager, phone, cancellationToken);

        if (user is not null && user.IsActive && user.PhoneNumberConfirmed
            && await OtpRateLimitHelper.IsWithinLimitsAsync(
                _codeRepository, _clock, _policy, user.Id, OtpPurpose.PhoneLogin, cancellationToken))
        {
            await _phoneOtpIssuer.IssueAsync(user, phone, OtpPurpose.PhoneLogin, cancellationToken);
        }

        return Result<SendPhoneLoginCodeResponse>.Success(
            new SendPhoneLoginCodeResponse("If your phone is registered, a login code has been sent."));
    }
}
