using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Timecrax.Api.Services;

public sealed class StorageImageService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StorageImageService> _logger;
    private readonly CloudinaryService _cloudinary;

    private static readonly HashSet<string> AllowedMime = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public StorageImageService(IConfiguration config, ILogger<StorageImageService> logger, CloudinaryService cloudinary)
    {
        _config = config;
        _logger = logger;
        _cloudinary = cloudinary;
    }

    public async Task<string> SaveThemeCoverFromDataUrlAsync(
        Guid themeId,
        string dataUrl,
        CancellationToken ct)
    {
        var (mime, bytes) = ParseDataUrlImage(dataUrl);

        if (!AllowedMime.Contains(mime))
            throw new InvalidOperationException("Formato de imagem não suportado (apenas jpeg/png/webp).");

        // valida imagem
        using var img = Image.Load(bytes);
        if (img.Width < 128 || img.Height < 128)
            throw new InvalidOperationException("Imagem muito pequena.");

        // Se Cloudinary está configurado, usa ele
        if (_cloudinary.IsEnabled)
        {
            var folder = $"timecrax/themes/{themeId}";
            return await _cloudinary.UploadFromDataUrlAsync(dataUrl, folder, "cover", ct);
        }

        // Fallback: storage local
        return await SaveToLocalStorageAsync(themeId, mime, img, ct);
    }

    public async Task<string> SaveCardImageFromDataUrlAsync(
        Guid themeId,
        int cardIndex,
        string slotKey,
        string dataUrl,
        CancellationToken ct)
    {
        var (mime, bytes) = ParseDataUrlImage(dataUrl);

        if (!AllowedMime.Contains(mime))
            throw new InvalidOperationException("Formato de imagem não suportado (apenas jpeg/png/webp).");

        // valida imagem
        using var img = Image.Load(bytes);

        // Se Cloudinary está configurado, usa ele
        if (_cloudinary.IsEnabled)
        {
            var folder = $"timecrax/themes/{themeId}/cards/{cardIndex}";
            var fileName = slotKey.Replace("/", "_");
            return await _cloudinary.UploadFromDataUrlAsync(dataUrl, folder, fileName, ct);
        }

        // Fallback: storage local
        return await SaveCardToLocalStorageAsync(themeId, cardIndex, slotKey, mime, img, ct);
    }

    public async Task<string> SaveCardImageFromFileAsync(
        Guid themeId,
        string relativePath,
        IFormFile file,
        CancellationToken ct)
    {
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Apenas imagens são permitidas.");

        using var img = await Image.LoadAsync(file.OpenReadStream(), ct);

        // Validação de tamanho mínimo
        if (img.Width < 128 || img.Height < 128)
            throw new InvalidOperationException("Imagem muito pequena (mínimo 128x128).");

        // Redimensiona se muito grande
        int maxDimension = 1200;
        if (img.Width > maxDimension || img.Height > maxDimension)
        {
            var ratio = Math.Min((double)maxDimension / img.Width, (double)maxDimension / img.Height);
            var newWidth = (int)(img.Width * ratio);
            var newHeight = (int)(img.Height * ratio);
            img.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Se Cloudinary está configurado, usa ele
        if (_cloudinary.IsEnabled)
        {
            var folder = $"timecrax/themes/{themeId}";
            var fileName = relativePath.Replace("/", "_").Replace("\\", "_").Replace(".webp", "");

            // Converte para bytes em memória
            using var ms = new MemoryStream();
            await img.SaveAsWebpAsync(ms, new WebpEncoder { Quality = 75 }, ct);
            var bytes = ms.ToArray();

            return await _cloudinary.UploadFromBytesAsync(bytes, folder, fileName, ct);
        }

        // Fallback: storage local
        return await SaveCardFileToLocalStorageAsync(themeId, relativePath, img, ct);
    }

    private async Task<string> SaveCardFileToLocalStorageAsync(Guid themeId, string relativePath, Image img, CancellationToken ct)
    {
        var root = _config["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath não configurado.");
        var publicBase = (_config["Storage:PublicBasePath"] ?? throw new InvalidOperationException("Storage:PublicBasePath não configurado."))
            .TrimEnd('/');

        var themeDir = Path.Combine(root, "themes", themeId.ToString());
        var fullPath = Path.Combine(themeDir, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await img.SaveAsWebpAsync(fs, new WebpEncoder { Quality = 75 }, ct);

        var urlPath = relativePath.Replace('\\', '/');
        return $"{publicBase}/themes/{themeId}/{urlPath}";
    }

    public async Task<string> SaveProfilePictureAsync(
        Guid userId,
        IFormFile file,
        CancellationToken ct)
    {
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Apenas imagens são permitidas.");

        // Se Cloudinary está configurado, usa ele
        if (_cloudinary.IsEnabled)
        {
            var folder = "timecrax/profile";
            var fileName = $"user_{userId}";
            return await _cloudinary.UploadFromFileAsync(file, folder, fileName, ct);
        }

        // Fallback: storage local
        return await SaveProfileToLocalStorageAsync(userId, file, ct);
    }

    public void TryDeleteThemeCover(Guid themeId)
    {
        if (_cloudinary.IsEnabled)
        {
            // Deleta do Cloudinary (async fire-and-forget)
            _ = _cloudinary.DeleteAsync($"timecrax/themes/{themeId}/cover");
            return;
        }

        // Fallback: storage local
        var root = _config["Storage:RootPath"];
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
        if (_cloudinary.IsEnabled)
        {
            // Deleta a pasta do Cloudinary (async fire-and-forget)
            _ = _cloudinary.DeleteFolderAsync($"timecrax/themes/{themeId}");
            return;
        }

        // Fallback: storage local
        var root = _config["Storage:RootPath"];
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
                _logger.LogWarning(ex, "Failed to delete theme folder for theme {ThemeId}", themeId);
            }
        }
    }

    #region Private Methods - Local Storage

    private async Task<string> SaveToLocalStorageAsync(Guid themeId, string mime, Image img, CancellationToken ct)
    {
        var root = _config["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath não configurado.");
        var publicBase = (_config["Storage:PublicBasePath"] ?? throw new InvalidOperationException("Storage:PublicBasePath não configurado."))
            .TrimEnd('/');

        var dir = Path.Combine(root, "themes", themeId.ToString());
        Directory.CreateDirectory(dir);

        var ext = MimeToExt(mime);
        var fileName = $"cover{ext}";
        var physicalPath = Path.Combine(dir, fileName);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        await using var fs = File.Create(physicalPath);

        IImageEncoder encoder = mime.ToLowerInvariant() switch
        {
            "image/jpeg" => new JpegEncoder { Quality = 75 },
            "image/png" => new PngEncoder(),
            "image/webp" => new WebpEncoder { Quality = 75 },
            _ => new WebpEncoder { Quality = 75 }
        };

        await img.SaveAsync(fs, encoder, ct);

        return $"{publicBase}/themes/{themeId}/{fileName}";
    }

    private async Task<string> SaveCardToLocalStorageAsync(Guid themeId, int cardIndex, string slotKey, string mime, Image img, CancellationToken ct)
    {
        var root = _config["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath não configurado.");
        var publicBase = (_config["Storage:PublicBasePath"] ?? throw new InvalidOperationException("Storage:PublicBasePath não configurado."))
            .TrimEnd('/');

        // slotKey pode ser algo como "main", "imageQuiz/options/0", etc.
        var relativePath = slotKey.Replace("/", Path.DirectorySeparatorChar.ToString());
        var dir = Path.Combine(root, "themes", themeId.ToString(), "cards", cardIndex.ToString(), Path.GetDirectoryName(relativePath) ?? "");
        Directory.CreateDirectory(dir);

        var ext = MimeToExt(mime);
        var fileName = $"{Path.GetFileName(relativePath)}{ext}";
        var physicalPath = Path.Combine(dir, fileName);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        await using var fs = File.Create(physicalPath);

        IImageEncoder encoder = new WebpEncoder { Quality = 75 };
        await img.SaveAsync(fs, encoder, ct);

        var urlPath = slotKey.Replace("\\", "/");
        return $"{publicBase}/themes/{themeId}/cards/{cardIndex}/{urlPath}{ext}";
    }

    private async Task<string> SaveProfileToLocalStorageAsync(Guid userId, IFormFile file, CancellationToken ct)
    {
        var root = _config["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath não configurado.");
        var publicBase = (_config["Storage:PublicBasePath"] ?? throw new InvalidOperationException("Storage:PublicBasePath não configurado."))
            .TrimEnd('/');

        var dir = Path.Combine(root, "profile");
        Directory.CreateDirectory(dir);

        var fileName = $"user_{userId}.webp";
        var physicalPath = Path.Combine(dir, fileName);

        using var img = await Image.LoadAsync(file.OpenReadStream(), ct);

        if (img.Width < 128 || img.Height < 128)
            throw new InvalidOperationException("Imagem muito pequena (mínimo 128x128).");

        await using var fs = File.Create(physicalPath);
        await img.SaveAsWebpAsync(fs, new WebpEncoder { Quality = 75 }, ct);

        return $"{publicBase}/profile/{fileName}";
    }

    #endregion

    #region Helper Methods

    private static (string mime, byte[] bytes) ParseDataUrlImage(string dataUrl)
    {
        var s = dataUrl.Trim();
        if (!s.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Theme.Image deve ser um dataUrl de imagem.");

        var comma = s.IndexOf(',');
        if (comma < 0) throw new InvalidOperationException("DataUrl inválido.");

        var meta = s[..comma];
        var b64 = s[(comma + 1)..];

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
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".webp"
    };

    #endregion
}
