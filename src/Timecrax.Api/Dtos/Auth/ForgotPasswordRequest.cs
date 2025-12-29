namespace Timecrax.Api.Dtos.Auth;

public record ForgotPasswordRequest(string Email, string? Language = "en");
