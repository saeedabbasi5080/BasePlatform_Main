using BasePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Sms;

/// <summary>
/// Placeholder SMS sender. In development (or when LogToConsole is true) logs the message
/// instead of calling an external provider. Replace the send body when wiring a real gateway.
/// </summary>
public sealed class ConsoleSmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ConsoleSmsService> _logger;

    public ConsoleSmsService(
        IOptions<SmsOptions> options,
        IHostEnvironment environment,
        ILogger<ConsoleSmsService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (ShouldLogInsteadOfSend())
        {
            var devLog =
                $"[DEV SMS via {_options.Provider}] To: {phoneNumber}{Environment.NewLine}{message}";

            _logger.LogWarning("{DevSmsMessage}", devLog);
            Console.WriteLine();
            Console.WriteLine("========== DEV SMS ==========");
            Console.WriteLine($"Provider: {_options.Provider}");
            Console.WriteLine($"From: {_options.FromNumber}");
            Console.WriteLine($"To: {phoneNumber}");
            Console.WriteLine(message);
            Console.WriteLine("=============================");
            Console.WriteLine();
            return Task.CompletedTask;
        }

        // TODO: integrate real SMS provider using ApiKey/ApiSecret/FromNumber.
        throw new InvalidOperationException(
            "Real SMS delivery is not configured. Set Sms:LogToConsole=true for development " +
            "or implement your provider in ConsoleSmsService.");
    }

    private bool ShouldLogInsteadOfSend() =>
        SmsSendMode.ShouldLogInsteadOfSend(_options, _environment);
}
