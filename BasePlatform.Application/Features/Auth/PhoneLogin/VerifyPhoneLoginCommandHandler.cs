using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed class VerifyPhoneLoginCommandHandler
    : ICommandHandler<VerifyPhoneLoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;
    private readonly AuthTokenFactory _tokenFactory;

    public VerifyPhoneLoginCommandHandler(
        UserManager<AppUser> userManager,
        IEmailVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        AuthTokenFactory tokenFactory)
    {
        _userManager = userManager;
        _codeRepository = codeRepository;
        _codeService = codeService;
        _clock = clock;
        _policy = policy;
        _tokenFactory = tokenFactory;
    }

    public async Task<Result<LoginResponse>> HandleAsync(
        VerifyPhoneLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!PhoneNumberNormalizer.TryNormalize(command.PhoneNumber, out var phone))
            return Result<LoginResponse>.Failure(
                Error.Validation("Auth.InvalidPhone", "Enter a valid mobile number (09XXXXXXXXX)."));

        var user = await PhoneUserLookup.FindByPhoneAsync(_userManager, phone, cancellationToken);
        if (user is null || !user.IsActive || !user.PhoneNumberConfirmed)
            return Result<LoginResponse>.Failure(OtpVerificationHelper.InvalidCodeError);

        var verifyResult = await OtpVerificationHelper.VerifyAsync(
            _codeRepository,
            _codeService,
            _clock,
            _policy,
            user.Id,
            command.Code.Trim(),
            OtpPurpose.PhoneLogin,
            consumeOnSuccess: true,
            cancellationToken);

        if (!verifyResult.IsSuccess)
            return Result<LoginResponse>.Failure(verifyResult.Error!);

        await _userManager.ResetAccessFailedCountAsync(user);
        return await _tokenFactory.CreateLoginResponseAsync(user, cancellationToken);
    }
}
