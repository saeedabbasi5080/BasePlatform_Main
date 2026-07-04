using BasePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Email;

/// <summary>Exposes <see cref="EmailVerificationOptions"/> as the Application-layer policy.</summary>
public sealed class EmailVerificationPolicyProvider : IEmailVerificationPolicy
{
    private readonly EmailVerificationOptions _options;

    public EmailVerificationPolicyProvider(IOptions<EmailVerificationOptions> options)
    {
        _options = options.Value;
    }

    public int CodeLength => _options.CodeLength;
    public TimeSpan CodeLifetime => TimeSpan.FromMinutes(_options.CodeLifetimeMinutes);
    public TimeSpan PasswordResetCodeLifetime => TimeSpan.FromMinutes(_options.PasswordResetLifetimeMinutes);
    public TimeSpan ResendCooldown => TimeSpan.FromSeconds(_options.ResendCooldownSeconds);
    public int MaxCodesPerHour => _options.MaxCodesPerHour;
    public int MaxVerifyAttempts => _options.MaxVerifyAttempts;
}
