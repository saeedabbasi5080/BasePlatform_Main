namespace BasePlatform.Infrastructure.Sms;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";

    public string Provider { get; set; } = "your-sms-provider";
    public string ApiKey { get; set; } = "your-api-key";
    public string ApiSecret { get; set; } = "your-api-secret";
    public string FromNumber { get; set; } = "+989000000000";
    public bool LogToConsole { get; set; }

    /// <summary>
    /// When set (e.g. unverified Kavenegar account), all SMS are sent to this number
    /// via api.Send(sender, overrideReceptor, message) while OTP stays tied to the requested phone.
    /// </summary>
    public string? OverrideReceptor { get; set; }

    /// <summary>
    /// When non-empty, only these phones may request OTP codes (dev / sandbox).
    /// </summary>
    public List<string> AllowedReceptors { get; set; } = [];
}
