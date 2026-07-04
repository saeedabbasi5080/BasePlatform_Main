using System.Net;
using System.Net.Mail;
using BasePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        IHostEnvironment environment,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        // DEV ONLY: write the email (including any OTP in the body) to the log instead of sending.
        if (ShouldLogInsteadOfSend())
        {
            var devLog =
                $"[DEV EMAIL] To: {to} | Subject: {subject}{Environment.NewLine}{body}";

            _logger.LogWarning("{DevEmailMessage}", devLog);
            Console.WriteLine();
            Console.WriteLine("========== DEV EMAIL (OTP) ==========");
            Console.WriteLine(devLog);
            Console.WriteLine("=====================================");
            Console.WriteLine();
            return;
        }

        // Sender address: explicit FromEmail, otherwise the authenticated username (which is an
        // email for most providers).
        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.Username
            : _options.FromEmail;

        // No SMTP host/sender configured (e.g. local/dev). Skip sending rather than crash the flow.
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning(
                "Email not sent to {Recipient} ('{Subject}'): SMTP is not configured.", to, subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);

        // EnableSsl drives STARTTLS on the standard submission port 587. Implicit-SSL (port 465)
        // is not supported by System.Net.Mail; use 587 with UseSsl=true.
        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {Recipient} ('{Subject}').", to, subject);
        }
        catch (Exception ex)
        {
            // Don't fail the calling use-case (registration, password reset) because of a transient
            // mail failure; log it for diagnostics instead.
            _logger.LogError(ex, "Failed to send email to {Recipient} ('{Subject}').", to, subject);
        }
    }

    private bool ShouldLogInsteadOfSend() =>
        _options.LogToConsole ||
        (_environment.IsDevelopment() && EmailOptions.IsPlaceholderSmtp(_options.SmtpHost));
}
