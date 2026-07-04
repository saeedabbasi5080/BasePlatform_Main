using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Common.Abstractions;

public interface IEmailVerificationCodeRepository
{
    Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken = default);

    /// <summary>Latest not-yet-consumed code for the user and purpose (may still be expired).</summary>
    Task<EmailVerificationCode?> GetLatestActiveAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    /// <summary>Marks all not-yet-consumed codes for the user and purpose as consumed.</summary>
    Task InvalidateActiveAsync(
        Guid userId,
        OtpPurpose purpose,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetLastCreatedAtAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    Task<int> CountCreatedSinceAsync(
        Guid userId,
        OtpPurpose purpose,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
