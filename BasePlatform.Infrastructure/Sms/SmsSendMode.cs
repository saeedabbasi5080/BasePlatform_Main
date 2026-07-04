using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Sms;

internal static class SmsSendMode
{
    internal static bool ShouldLogInsteadOfSend(
        SmsOptions options,
        IHostEnvironment environment)
    {
        if (options.LogToConsole)
            return true;

        if (!environment.IsDevelopment())
            return false;

        return string.IsNullOrWhiteSpace(options.ApiKey)
               || options.ApiKey.Contains("your-", StringComparison.OrdinalIgnoreCase)
               || options.Provider.Contains("your-", StringComparison.OrdinalIgnoreCase);
    }
}
