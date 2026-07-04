using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.PhoneRegister;

public sealed class SendPhoneRegisterCodeCommandHandler
    : ICommandHandler<SendPhoneRegisterCodeCommand, Result<SendPhoneRegisterCodeResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly PhoneOtpIssuer _phoneOtpIssuer;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;
    private readonly ISmsOtpPolicy _smsOtpPolicy;

    public SendPhoneRegisterCodeCommandHandler(
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

    public async Task<Result<SendPhoneRegisterCodeResponse>> HandleAsync(
        SendPhoneRegisterCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!PhoneNumberNormalizer.TryNormalize(command.PhoneNumber, out var phone))
            return Result<SendPhoneRegisterCodeResponse>.Failure(
                Error.Validation("Auth.InvalidPhone", "Enter a valid mobile number (09XXXXXXXXX)."));

        if (!_smsOtpPolicy.IsPhoneAllowed(phone))
            return Result<SendPhoneRegisterCodeResponse>.Failure(
                Error.Validation(
                    "Auth.PhoneNotAllowed",
                    "SMS is limited to the verified test number (09195200872) until Kavenegar account verification completes."));

        var existing = await PhoneUserLookup.FindByPhoneAsync(_userManager, phone, cancellationToken);
        if (existing is not null && existing.PhoneNumberConfirmed)
            return Result<SendPhoneRegisterCodeResponse>.Failure(
                Error.Conflict("Auth.PhoneTaken", "This phone number is already registered."));

        AppUser user;
        if (existing is not null)
        {
            user = existing;
            user.FirstName = command.FirstName.Trim();
            user.LastName = command.LastName.Trim();
            user.DisplayName = $"{user.FirstName} {user.LastName}";
            user.UpdatedAt = _clock.UtcNow;
            await _userManager.UpdateAsync(user);
        }
        else
        {
            var syntheticEmail = PhoneUserLookup.BuildSyntheticEmail(phone);
            if (await _userManager.FindByEmailAsync(syntheticEmail) is not null)
                return Result<SendPhoneRegisterCodeResponse>.Failure(
                    Error.Conflict("Auth.PhoneTaken", "This phone number is already registered."));

            user = new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = command.FirstName.Trim(),
                LastName = command.LastName.Trim(),
                DisplayName = $"{command.FirstName.Trim()} {command.LastName.Trim()}",
                Email = syntheticEmail,
                UserName = phone,
                PhoneNumber = phone,
                IsActive = true,
                CreatedAt = _clock.UtcNow,
                UpdatedAt = _clock.UtcNow
            };

            var password = Guid.NewGuid().ToString("N") + "Aa1!";
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return Result<SendPhoneRegisterCodeResponse>.Failure(
                    Error.Validation("Auth.RegistrationFailed", errorMessage));
            }

            await _userManager.AddToRoleAsync(user, "User");
        }

        if (!await OtpRateLimitHelper.IsWithinLimitsAsync(
                _codeRepository, _clock, _policy, user.Id, OtpPurpose.PhoneRegistration, cancellationToken))
        {
            return Result<SendPhoneRegisterCodeResponse>.Failure(
                Error.Validation("Auth.RateLimited", "Please wait before requesting another code."));
        }

        await _phoneOtpIssuer.IssueAsync(user, phone, OtpPurpose.PhoneRegistration, cancellationToken);

        return Result<SendPhoneRegisterCodeResponse>.Success(
            new SendPhoneRegisterCodeResponse("Verification code sent to your phone."));
    }
}
