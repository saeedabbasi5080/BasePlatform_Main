namespace BasePlatform.Application.Common.Abstractions;

/// <summary>
/// Runtime policy for email-verification OTP codes (configurable via appsettings).
/// </summary>
public interface IEmailVerificationPolicy
{
    /// <summary>Number of digits in a generated code.</summary>
    int CodeLength { get; }

    /// <summary>How long a code remains valid after it is issued.</summary>
    TimeSpan CodeLifetime { get; }

    /// <summary>How long a password-reset OTP remains valid after it is issued.</summary>
    TimeSpan PasswordResetCodeLifetime { get; }

    /// <summary>Minimum time between two resend requests for the same account.</summary>
    TimeSpan ResendCooldown { get; }

    /// <summary>Maximum codes that may be generated for an account within a rolling hour.</summary>
    int MaxCodesPerHour { get; }

    /// <summary>Maximum verification attempts allowed against a single code.</summary>
    int MaxVerifyAttempts { get; }
}
