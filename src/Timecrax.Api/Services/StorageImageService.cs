using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace Timecrax.Api.Services;

public sealed class StorageImageService(IConfiguration config)
{
    private static readonly HashSet<string> AllowedMime = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public async Task<string> SaveThemeCoverFromDataUrlAsync(
        Guid themeId,
        string dataUrl,
        CancellationToken ct)
    {
        var (mime, bytes) = ParseDataUrlImage(dataUrl);

        if (!AllowedMime.Contains(mime))
            throw new InvalidOperationException("Formato de imagem não suportado (apenas jpeg/png/webp).");

        // valida imagem e normaliza (opcional, mas recomendado)
        using var img = Image.Load(bytes);
        if (img.Width < 128 || img.Height < 128)
            throw new InvalidOperationException("Imagem muito pequena.");

        var root = config["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath não configurado.");
        var publicBase = (config["Storage:PublicBasePath"] ?? throw new InvalidOperationException("Storage:PublicBasePath não configurado."))
            .TrimEnd('/');

        var dir = Path.Combine(root, "themes", themeId.ToString());
        Directory.CreateDirectory(dir);

        // sempre “cover.ext”
        var ext = MimeToExt(mime);
        var fileName = $"cover{ext}";
        var physicalPath = Path.Combine(dir, fileName);

        // sobrescreve a capa anterior
        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        await using var fs = File.Create(physicalPath);

        // você pode escolher sempre salvar em WEBP/JPG para padronizar.
        // aqui vou salvar "no formato do mime"
        IImageEncoder encoder = mime.ToLowerInvariant() switch
        {
            "image/jpeg" => new JpegEncoder { Quality = 75 },
            "image/png"  => new PngEncoder(),
            "image/webp" => new WebpEncoder { Quality = 75 },
            _ => new WebpEncoder { Quality = 75 }
        };

        await img.SaveAsync(fs, encoder, ct);

        var url = $"{publicBase}/themes/{themeId}/{fileName}";
        return url;
    }

    public void TryDeleteThemeCover(Guid themeId)
    {
        var root = config["Storage:RootPath"];
        if (string.IsNullOrWhiteSpace(root)) return;

        var dir = Path.Combine(root, "themes", themeId.ToString());

        foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".webp" })
        {
            var path = Path.Combine(dir, $"cover{ext}");
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public void DeleteThemeFolder(Guid themeId)
    {
        var root = config["Storage:RootPath"];
        if (string.IsNullOrWhiteSpace(root)) return;

        var themeDir = Path.Combine(root, "themes", themeId.ToString());

        if (Directory.Exists(themeDir))
        {
            try
            {
                Directory.Delete(themeDir, recursive: true);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - deleting files is not critical
                Console.WriteLine($"Warning: Failed to delete theme folder {themeId}: {ex.Message}");
            }
        }
    }

    private static (string mime, byte[] bytes) ParseDataUrlImage(string dataUrl)
    {
        // data:image/png;base64,AAAA...
        var s = dataUrl.Trim();
        if (!s.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Theme.Image deve ser um dataUrl de imagem.");

        var comma = s.IndexOf(',');
        if (comma < 0) throw new InvalidOperationException("DataUrl inválido.");

        var meta = s[..comma];           // data:image/png;base64
        var b64  = s[(comma + 1)..];     // AAAA...

        var semi = meta.IndexOf(';');
        var mime = semi >= 0 ? meta["data:".Length..semi] : meta["data:".Length..];

        if (!meta.Contains(";base64", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DataUrl deve estar em base64.");

        byte[] bytes;
        try { bytes = Convert.FromBase64String(b64); }
        catch { throw new InvalidOperationException("Base64 inválido."); }

        return (mime, bytes);
    }

    private static string MimeToExt(string mime) => mime.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png"  => ".png",
        "image/webp" => ".webp",
        _ => ".webp"
    };
}
