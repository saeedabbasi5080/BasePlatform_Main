using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.VerifyResetCode;

public sealed class VerifyResetCodeCommandHandler
    : ICommandHandler<VerifyResetCodeCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;

    public VerifyResetCodeCommandHandler(
        UserManager<AppUser> userManager,
        IEmailVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy)
    {
        _userManager = userManager;
        _codeRepository = codeRepository;
        _codeService = codeService;
        _clock = clock;
        _policy = policy;
    }

    public async Task<Result> HandleAsync(
        VerifyResetCodeCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null || !user.IsActive)
            return Result.Failure(OtpVerificationHelper.InvalidCodeError);

        // Validate without consuming so reset-password can apply the new password next.
        return await OtpVerificationHelper.VerifyAsync(
            _codeRepository,
            _codeService,
            _clock,
            _policy,
            user.Id,
            command.Code,
            OtpPurpose.PasswordReset,
            consumeOnSuccess: false,
            cancellationToken);
    }
}
