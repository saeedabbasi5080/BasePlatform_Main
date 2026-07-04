using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Sms;

internal sealed class SmsOtpPolicy : ISmsOtpPolicy
{
    private readonly SmsOptions _options;

    public SmsOtpPolicy(IOptions<SmsOptions> options)
    {
        _options = options.Value;
    }

    public bool IsPhoneAllowed(string normalizedPhone)
    {
        if (_options.AllowedReceptors.Count == 0)
            return true;

        foreach (var allowed in _options.AllowedReceptors)
        {
            if (PhoneNumberNormalizer.TryNormalize(allowed, out var normalized)
                && normalized == normalizedPhone)
            {
                return true;
            }
        }

        return false;
    }
}
