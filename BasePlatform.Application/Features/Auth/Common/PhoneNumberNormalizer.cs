using System.Text.RegularExpressions;

namespace BasePlatform.Application.Features.Auth.Common;

public static partial class PhoneNumberNormalizer
{
    [GeneratedRegex(@"^09\d{9}$")]
    private static partial Regex IranMobileRegex();

    /// <summary>
    /// Normalizes Iranian mobile numbers to <c>09XXXXXXXXX</c> (11 digits).
    /// Accepts formats like 0912..., +98912..., 98912...
    /// </summary>
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("98") && digits.Length == 12)
            digits = "0" + digits[2..];
        else if (digits.StartsWith("9") && digits.Length == 10)
            digits = "0" + digits;

        if (!IranMobileRegex().IsMatch(digits))
            return false;

        normalized = digits;
        return true;
    }
}
