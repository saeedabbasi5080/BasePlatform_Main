using BasePlatform.Domain.Enums;

namespace BasePlatform.Domain.Entities;

/// <summary>
/// A short-lived, single-use numeric OTP (email confirmation or password reset).
/// Only the hash of the code is stored.
/// </summary>
public class EmailVerificationCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OtpPurpose Purpose { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public int AttemptCount { get; set; }

    public AppUser User { get; set; } = null!;

    public bool IsConsumed => ConsumedAt.HasValue;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsUsable(DateTimeOffset now) => !IsConsumed && !IsExpired(now);
}
