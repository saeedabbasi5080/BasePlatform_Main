namespace BasePlatform.Application.Common.Abstractions;

public interface ISmsOtpPolicy
{
    bool IsPhoneAllowed(string normalizedPhone);
}
