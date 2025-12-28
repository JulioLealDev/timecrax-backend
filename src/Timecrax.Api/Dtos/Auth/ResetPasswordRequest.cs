namespace Timecrax.Api.Dtos.Auth;

public record ResetPasswordRequest(
    string Token,
    string NewPassword
);
