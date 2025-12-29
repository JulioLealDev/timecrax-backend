namespace Timecrax.Api.Dtos.Auth;

public record RegisterRequest(
    string Role,        // "student" | "teacher" | "player"
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? SchoolName,
    string? Language    // "en" | "pt-br" | "pt-pt" | "fr" | "es"
);
