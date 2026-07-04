namespace BasePlatform.Infrastructure.Email;

/// <summary>
/// Bound from the "EmailVerification" configuration section. All values have sensible
/// defaults so the section is optional.
/// </summary>
public sealed class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    /// <summary>Number of digits in a generated code.</summary>
    public int CodeLength { get; set; } = 6;

    /// <summary>How long a code remains valid after it is issued, in minutes.</summary>
    public int CodeLifetimeMinutes { get; set; } = 10;

    /// <summary>How long a password-reset OTP remains valid, in minutes.</summary>
    public int PasswordResetLifetimeMinutes { get; set; } = 15;

    /// <summary>Minimum time between two resend requests for the same account, in seconds.</summary>
    public int ResendCooldownSeconds { get; set; } = 60;

    /// <summary>Maximum codes that may be generated for an account within a rolling hour.</summary>
    public int MaxCodesPerHour { get; set; } = 5;

    /// <summary>Maximum verification attempts allowed against a single code.</summary>
    public int MaxVerifyAttempts { get; set; } = 5;
}
