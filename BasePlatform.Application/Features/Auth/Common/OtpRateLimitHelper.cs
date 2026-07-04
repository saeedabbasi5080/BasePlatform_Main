using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Features.Auth.Common;

public static class OtpRateLimitHelper
{
    public static async Task<bool> IsWithinLimitsAsync(
        IEmailVerificationCodeRepository repository,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;

        var lastCreatedAt = await repository.GetLastCreatedAtAsync(userId, purpose, cancellationToken);
        if (lastCreatedAt is not null && now - lastCreatedAt.Value < policy.ResendCooldown)
            return false;

        var sinceHour = now.AddHours(-1);
        var lastHourCount = await repository.CountCreatedSinceAsync(userId, purpose, sinceHour, cancellationToken);
        if (lastHourCount >= policy.MaxCodesPerHour)
            return false;

        return true;
    }
}
