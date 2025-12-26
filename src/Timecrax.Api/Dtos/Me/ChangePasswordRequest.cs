namespace Timecrax.Api.Dtos.Me;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
