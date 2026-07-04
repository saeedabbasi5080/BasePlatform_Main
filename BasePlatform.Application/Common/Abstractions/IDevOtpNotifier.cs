using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Common.Abstractions;

/// <summary>
/// DEV ONLY: surfaces the plain OTP during local development (never used in production flows).
/// </summary>
public interface IDevOtpNotifier
{
    void Notify(string email, string code, OtpPurpose purpose);

    void NotifySms(string phoneNumber, string code, OtpPurpose purpose);
}
