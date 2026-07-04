using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;

    public ConfirmEmailCommandHandler(
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
        ConfirmEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result.Failure(OtpVerificationHelper.InvalidCodeError);

        // Idempotent: confirming an already-confirmed account succeeds quietly.
        if (user.EmailConfirmed)
            return Result.Success();

        var verifyResult = await OtpVerificationHelper.VerifyAsync(
            _codeRepository,
            _codeService,
            _clock,
            _policy,
            user.Id,
            command.Code,
            OtpPurpose.EmailConfirmation,
            consumeOnSuccess: true,
            cancellationToken);

        if (verifyResult.IsFailure)
            return verifyResult;

        user.EmailConfirmed = true;
        user.UpdatedAt = _clock.UtcNow;
        await _codeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
