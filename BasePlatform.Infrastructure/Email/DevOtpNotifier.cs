using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasePlatform.Infrastructure.Email;

/// <summary>
/// Writes the latest OTP to a fixed file under the API content root so it is easy to find locally.
/// </summary>
public sealed class DevOtpNotifier : IDevOtpNotifier
{
    private const string DevOtpDirectoryName = "dev-emails";

    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevOtpNotifier> _logger;

    public DevOtpNotifier(IHostEnvironment environment, ILogger<DevOtpNotifier> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public void Notify(string email, string code, OtpPurpose purpose)
    {
        if (!_environment.IsDevelopment())
            return;

        var purposeLabel = purpose == OtpPurpose.PasswordReset ? "password reset" : "email confirmation";
        var endpoint = purpose == OtpPurpose.PasswordReset
            ? "POST /api/auth/verify-reset-code then POST /api/auth/reset-password"
            : "POST /api/auth/confirm-email";

        var filePath = WriteLatestOtpFile(
            purposeLabel,
            "Email: " + email,
            endpoint,
            code);

        _logger.LogWarning(
            "DEV OTP ({Purpose}) for {Email} written to {Path}. Code: {Code}",
            purposeLabel, email, filePath, code);

        Console.WriteLine();
        Console.WriteLine("========== DEV OTP ==========");
        Console.WriteLine($"Purpose: {purposeLabel}");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"OTP CODE: {code}");
        Console.WriteLine($"Saved to: {filePath}");
        Console.WriteLine("=============================");
        Console.WriteLine();
    }

    public void NotifySms(string phoneNumber, string code, OtpPurpose purpose)
    {
        if (!_environment.IsDevelopment())
            return;

        var purposeLabel = purpose switch
        {
            OtpPurpose.PhoneLogin => "phone login",
            OtpPurpose.PhoneRegistration => "phone registration",
            OtpPurpose.PasswordReset => "password reset",
            _ => "email confirmation"
        };
        var endpoint = purpose switch
        {
            OtpPurpose.PhoneLogin => "POST /api/auth/phone/login/verify",
            OtpPurpose.PhoneRegistration => "POST /api/auth/phone/register/verify",
            OtpPurpose.PasswordReset => "POST /api/auth/verify-reset-code",
            _ => "POST /api/auth/confirm-email"
        };

        var filePath = WriteLatestOtpFile(
            purposeLabel,
            "Phone: " + phoneNumber,
            endpoint,
            code);

        _logger.LogWarning(
            "DEV OTP ({Purpose}) for {Phone} written to {Path}. Code: {Code}",
            purposeLabel, phoneNumber, filePath, code);

        Console.WriteLine();
        Console.WriteLine("========== DEV OTP ==========");
        Console.WriteLine($"Purpose: {purposeLabel}");
        Console.WriteLine($"Phone: {phoneNumber}");
        Console.WriteLine($"OTP CODE: {code}");
        Console.WriteLine($"Saved to: {filePath}");
        Console.WriteLine("=============================");
        Console.WriteLine();
    }

    private string WriteLatestOtpFile(
        string purposeLabel,
        string recipientLine,
        string endpoint,
        string code)
    {
        var directory = Path.Combine(_environment.ContentRootPath, DevOtpDirectoryName);
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, "latest-otp.txt");
        var contents =
            "Generated at (UTC): " + DateTimeOffset.UtcNow.ToString("O") + Environment.NewLine +
            "Purpose: " + purposeLabel + Environment.NewLine +
            recipientLine + Environment.NewLine +
            "OTP CODE: " + code + Environment.NewLine + Environment.NewLine +
            "Use in " + endpoint;

        File.WriteAllText(filePath, contents);
        return filePath;
    }
}
