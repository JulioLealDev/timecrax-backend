namespace Timecrax.Api.Dtos.Me;

public sealed class ChangeEmailRequest
{
    public string CurrentPassword { get; set; } = default!;
    public string NewEmail { get; set; } = default!;
}
