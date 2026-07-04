namespace BasePlatform.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// DEV ONLY: when true, emails are written to the log (console + file) instead of being sent
    /// over SMTP. Useful for reading OTP codes locally without a mail server. Keep false in production.
    /// </summary>
    public bool LogToConsole { get; set; }

    /// <summary>
    /// True when the SMTP host is empty or still uses a template placeholder from appsettings.
    /// In Development, placeholder hosts automatically log emails instead of sending via SMTP.
    /// </summary>
    public static bool IsPlaceholderSmtp(string? smtpHost)
    {
        if (string.IsNullOrWhiteSpace(smtpHost))
            return true;

        return smtpHost.Contains("yourprovider", StringComparison.OrdinalIgnoreCase)
            || smtpHost.Contains("example.com", StringComparison.OrdinalIgnoreCase);
    }
}
