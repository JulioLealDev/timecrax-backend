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

    /// <summary>
    /// Validates password complexity requirements:
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// </summary>
    public static PasswordValidationResult ValidatePassword(this string? password)
    {
        if (string.IsNullOrEmpty(password))
            return new PasswordValidationResult(false, "PASSWORD_REQUIRED");

        if (password.Length < 8)
            return new PasswordValidationResult(false, "PASSWORD_TOO_SHORT");

        if (!password.Any(char.IsUpper))
            return new PasswordValidationResult(false, "PASSWORD_NO_UPPERCASE");

        if (!password.Any(char.IsLower))
            return new PasswordValidationResult(false, "PASSWORD_NO_LOWERCASE");

        if (!password.Any(char.IsDigit))
            return new PasswordValidationResult(false, "PASSWORD_NO_DIGIT");

        return new PasswordValidationResult(true, null);
    }
}

public record PasswordValidationResult(bool IsValid, string? ErrorCode);
