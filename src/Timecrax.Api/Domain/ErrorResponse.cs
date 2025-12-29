namespace Timecrax.Api.Domain;

/// <summary>
/// Standardized error response format for API errors.
/// Use 'code' for i18n-friendly error codes, 'errors' for validation errors.
/// </summary>
public record ErrorResponse(string Code)
{
    public static object Single(string code) => new { code };

    public static object Validation(Dictionary<string, string> errors) => new { errors };

    public static object Validation(string field, string code) =>
        new { errors = new Dictionary<string, string> { [field] = code } };
}

/// <summary>
/// Common error codes used across the API.
/// </summary>
public static class ErrorCodes
{
    // Auth errors
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string InvalidRole = "INVALID_ROLE";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string TokenRequired = "TOKEN_REQUIRED";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string TokenAlreadyUsed = "TOKEN_ALREADY_USED";
    public const string EmailInUse = "EMAIL_IN_USE";
    public const string TooManyRequests = "TOO_MANY_REQUESTS";

    // Password errors
    public const string PasswordRequired = "PASSWORD_REQUIRED";
    public const string PasswordTooShort = "PASSWORD_TOO_SHORT";
    public const string CurrentPasswordRequired = "CURRENT_PASSWORD_REQUIRED";
    public const string InvalidCurrentPassword = "INVALID_CURRENT_PASSWORD";
    public const string InvalidPassword = "INVALID_PASSWORD";
    public const string SamePassword = "SAME_PASSWORD";
    public const string NewPasswordTooShort = "NEW_PASSWORD_TOO_SHORT";

    // User errors
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string FirstNameTooShort = "FIRST_NAME_TOO_SHORT";
    public const string LastNameTooShort = "LAST_NAME_TOO_SHORT";
    public const string SchoolNameRequired = "SCHOOL_NAME_REQUIRED";

    // Email errors
    public const string NewEmailRequired = "NEW_EMAIL_REQUIRED";
    public const string SameEmail = "SAME_EMAIL";

    // File errors
    public const string InvalidFile = "INVALID_FILE";
    public const string InvalidImage = "INVALID_IMAGE";
    public const string ImageTooSmall = "IMAGE_TOO_SMALL";
    public const string OnlyImagesAllowed = "ONLY_IMAGES_ALLOWED";

    // Generic errors
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string ServerError = "SERVER_ERROR";
}
