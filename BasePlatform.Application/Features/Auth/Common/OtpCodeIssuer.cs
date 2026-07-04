using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Features.Auth.Common;

/// <summary>
/// Issues OTP codes for email confirmation or password reset: invalidates prior codes for the
/// same purpose, stores the hash, and queues the email asynchronously.
/// </summary>
public sealed class OtpCodeIssuer
{
    private readonly IEmailVerificationCodeRepository _repository;
    private readonly IVerificationCodeService _codeService;
    private readonly IEmailQueue _emailQueue;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;
    private readonly IDevOtpNotifier _devOtpNotifier;

    public OtpCodeIssuer(
        IEmailVerificationCodeRepository repository,
        IVerificationCodeService codeService,
        IEmailQueue emailQueue,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        IDevOtpNotifier devOtpNotifier)
    {
        _repository = repository;
        _codeService = codeService;
        _emailQueue = emailQueue;
        _clock = clock;
        _policy = policy;
        _devOtpNotifier = devOtpNotifier;
    }

    public async Task IssueAsync(
        AppUser user,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var lifetime = purpose switch
        {
            OtpPurpose.PasswordReset => _policy.PasswordResetCodeLifetime,
            _ => _policy.CodeLifetime
        };

        var code = _codeService.Generate();
        // Write the plain code locally before any DB work so dev testing still works if persistence fails.
        _devOtpNotifier.Notify(user.Email!, code, purpose);

        await _repository.InvalidateActiveAsync(user.Id, purpose, now, cancellationToken);

        await _repository.AddAsync(new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = purpose,
            CodeHash = _codeService.Hash(user.Id, code),
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        var minutes = (int)lifetime.TotalMinutes;
        var (subject, body) = purpose switch
        {
            OtpPurpose.PasswordReset => (
                "Your password reset code",
                $"Your password reset code is {code}. It expires in {minutes} minutes. " +
                "If you didn't request this, you can ignore this email."),
            _ => (
                "Your verification code",
                $"Your verification code is {code}. It expires in {minutes} minutes. " +
                "If you didn't request this, you can ignore this email.")
        };

        await _emailQueue.EnqueueAsync(
            new EmailMessage(user.Email!, subject, body, IsHtml: false),
            cancellationToken);
    }
}
