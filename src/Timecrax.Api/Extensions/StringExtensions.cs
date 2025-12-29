using System.Text.RegularExpressions;

namespace Timecrax.Api.Extensions;

public static partial class StringExtensions
{
    // RFC 5322 simplified email regex - validates common email formats
    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public static bool IsValidEmail(this string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Basic length check
        if (email.Length < 5 || email.Length > 254)
            return false;

        return EmailRegex().IsMatch(email);
    }
}
