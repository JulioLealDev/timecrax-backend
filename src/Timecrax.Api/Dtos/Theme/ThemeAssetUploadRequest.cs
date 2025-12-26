using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Timecrax.Api.Dtos.ThemeAssets;

public sealed class ThemeAssetUploadRequest
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = default!;

    [FromForm(Name = "slotKey")]
    public string SlotKey { get; set; } = default!;
}
