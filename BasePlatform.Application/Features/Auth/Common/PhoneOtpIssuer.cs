using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Features.Auth.Common;

/// <summary>
/// Issues OTP codes for phone login/registration and queues SMS delivery asynchronously.
/// </summary>
public sealed class PhoneOtpIssuer
{
    private readonly IEmailVerificationCodeRepository _repository;
    private readonly IVerificationCodeService _codeService;
    private readonly ISmsQueue _smsQueue;
    private readonly IDateTimeProvider _clock;
    private readonly IEmailVerificationPolicy _policy;
    private readonly IDevOtpNotifier _devOtpNotifier;

    public PhoneOtpIssuer(
        IEmailVerificationCodeRepository repository,
        IVerificationCodeService codeService,
        ISmsQueue smsQueue,
        IDateTimeProvider clock,
        IEmailVerificationPolicy policy,
        IDevOtpNotifier devOtpNotifier)
    {
        _repository = repository;
        _codeService = codeService;
        _smsQueue = smsQueue;
        _clock = clock;
        _policy = policy;
        _devOtpNotifier = devOtpNotifier;
    }

    public async Task IssueAsync(
        AppUser user,
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var code = _codeService.Generate();
        _devOtpNotifier.NotifySms(phoneNumber, code, purpose);

        await _repository.InvalidateActiveAsync(user.Id, purpose, now, cancellationToken);

        await _repository.AddAsync(new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = purpose,
            CodeHash = _codeService.Hash(user.Id, code),
            CreatedAt = now,
            ExpiresAt = now.Add(_policy.CodeLifetime)
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        var minutes = (int)_policy.CodeLifetime.TotalMinutes;
        var purposeLabel = purpose == OtpPurpose.PhoneLogin ? "ورود" : "ثبت‌نام";
        var body =
            $"کد {purposeLabel} سینا: {code}\n" +
            $"اعتبار: {minutes} دقیقه\n" +
            "اگر درخواست نداده‌اید، این پیام را نادیده بگیرید.";

        await _smsQueue.EnqueueAsync(new SmsMessage(phoneNumber, body), cancellationToken);
    }
}
