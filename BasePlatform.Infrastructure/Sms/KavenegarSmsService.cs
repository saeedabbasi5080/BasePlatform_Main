using BasePlatform.Application.Common.Abstractions;
using Kavenegar;
using Kavenegar.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Sms;

public sealed class KavenegarSmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<KavenegarSmsService> _logger;

    public KavenegarSmsService(
        IOptions<SmsOptions> options,
        IHostEnvironment environment,
        ILogger<KavenegarSmsService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (SmsSendMode.ShouldLogInsteadOfSend(_options, _environment))
        {
            LogDevSms(phoneNumber, message);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.FromNumber))
        {
            throw new InvalidOperationException(
                "Sms:FromNumber is required for Kavenegar (e.g. 2000660110).");
        }

        try
        {
            var api = new KavenegarApi(_options.ApiKey.Trim());
            var sender = _options.FromNumber.Trim();
            var requestedReceptor = phoneNumber.Trim();
            var receptor = ResolveReceptor(requestedReceptor);

            await Task.Run(
                () => api.Send(sender, receptor, message),
                cancellationToken);

            if (receptor != requestedReceptor)
            {
                _logger.LogInformation(
                    "Kavenegar SMS redirected from {RequestedPhone} to {OverridePhone} via sender {Sender}.",
                    requestedReceptor,
                    receptor,
                    sender);
            }
            else
            {
                _logger.LogInformation(
                    "Kavenegar SMS sent to {Phone} via sender {Sender}.",
                    receptor,
                    sender);
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Kavenegar API error while sending SMS to {Phone}.", phoneNumber);
            throw new InvalidOperationException($"Kavenegar API error: {ex.Message}", ex);
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Kavenegar HTTP error while sending SMS to {Phone}.", phoneNumber);
            throw new InvalidOperationException($"Kavenegar connection error: {ex.Message}", ex);
        }
    }

    private string ResolveReceptor(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(_options.OverrideReceptor))
            return phoneNumber;

        return _options.OverrideReceptor.Trim();
    }

    private void LogDevSms(string phoneNumber, string message)
    {
        var receptor = ResolveReceptor(phoneNumber);
        var devLog =
            receptor == phoneNumber
                ? $"[DEV SMS via Kavenegar] To: {phoneNumber}{Environment.NewLine}{message}"
                : $"[DEV SMS via Kavenegar] Requested: {phoneNumber}, sent to override: {receptor}{Environment.NewLine}{message}";

        _logger.LogWarning("{DevSmsMessage}", devLog);
        Console.WriteLine();
        Console.WriteLine("========== DEV SMS ==========");
        Console.WriteLine($"Provider: Kavenegar");
        Console.WriteLine($"From: {_options.FromNumber}");
        Console.WriteLine($"Requested: {phoneNumber}");
        if (receptor != phoneNumber)
            Console.WriteLine($"Override To: {receptor}");
        else
            Console.WriteLine($"To: {phoneNumber}");
        Console.WriteLine(message);
        Console.WriteLine("=============================");
        Console.WriteLine();
    }
}
