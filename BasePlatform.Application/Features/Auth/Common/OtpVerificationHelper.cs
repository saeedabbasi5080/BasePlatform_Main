using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.Common;

public static class OtpErrorCodes
{
    public const string InvalidCode = "INVALID_CODE";
    public const string ExpiredCode = "EXPIRED_CODE";
}

public static class OtpVerificationHelper
{
    public static readonly Error InvalidCodeError = Error.Validation(
        OtpErrorCodes.InvalidCode, "The verification code is invalid.");

    public static readonly Error ExpiredCodeError = Error.Validation(
        OtpErrorCodes.ExpiredCode, "The verification code has expired.");

    /// <summary>
    /// Validates an OTP for the given user/purpose. On success the code is marked consumed unless
    /// <paramref name="consumeOnSuccess"/> is false.
    /// </summary>
    public static async Task<Result> VerifyAsync(
        IEmailVerificationCodeRepository repository,
        IVerificationCodeService codeService,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        Guid userId,
        string code,
        OtpPurpose purpose,
        bool consumeOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var record = await repository.GetLatestActiveAsync(userId, purpose, cancellationToken);

        if (record is null)
            return Result.Failure(InvalidCodeError);

        if (record.IsExpired(now))
            return Result.Failure(ExpiredCodeError);

        record.AttemptCount++;

        if (record.AttemptCount > policy.MaxVerifyAttempts)
        {
            record.ConsumedAt = now;
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Failure(InvalidCodeError);
        }

        if (!codeService.Verify(userId, code, record.CodeHash))
        {
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Failure(InvalidCodeError);
        }

        if (consumeOnSuccess)
            record.ConsumedAt = now;

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
