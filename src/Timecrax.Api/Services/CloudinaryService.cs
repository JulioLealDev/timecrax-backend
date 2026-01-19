using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Timecrax.Api.Services;

public sealed class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly bool _isEnabled;

    public CloudinaryService(IConfiguration config, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudName = ResolveEnvVar(config["Cloudinary:CloudName"]);
        var apiKey = ResolveEnvVar(config["Cloudinary:ApiKey"]);
        var apiSecret = ResolveEnvVar(config["Cloudinary:ApiSecret"]);

        // Se todas as credenciais estão configuradas, habilita o Cloudinary
        if (!string.IsNullOrWhiteSpace(cloudName) &&
            !string.IsNullOrWhiteSpace(apiKey) &&
            !string.IsNullOrWhiteSpace(apiSecret))
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _isEnabled = true;
            _logger.LogInformation("Cloudinary enabled with cloud name: {CloudName}", cloudName);
        }
        else
        {
            _isEnabled = false;
            _logger.LogWarning("Cloudinary not configured - falling back to local storage");
        }
    }

    private static string? ResolveEnvVar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        return System.Text.RegularExpressions.Regex.Replace(value, @"\$\{(\w+)\}", match =>
        {
            var envVarName = match.Groups[1].Value;
            return Environment.GetEnvironmentVariable(envVarName) ?? "";
        });
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Faz upload de uma imagem a partir de um dataUrl (base64)
    /// </summary>
    /// <param name="dataUrl">Data URL da imagem (data:image/png;base64,...)</param>
    /// <param name="folder">Pasta no Cloudinary (ex: "themes/guid-id")</param>
    /// <param name="fileName">Nome do arquivo sem extensão (ex: "cover")</param>
    /// <returns>URL pública da imagem no Cloudinary</returns>
    public async Task<string> UploadFromDataUrlAsync(string dataUrl, string folder, string fileName, CancellationToken ct = default)
    {
        if (!_isEnabled || _cloudinary == null)
            throw new InvalidOperationException("Cloudinary is not configured");

        var bytes = ParseDataUrlToBytes(dataUrl);

        using var stream = new MemoryStream(bytes);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = fileName,
            Overwrite = true,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {Url}", result.SecureUrl);
        return result.SecureUrl.ToString();
    }

    /// <summary>
    /// Faz upload de uma imagem a partir de um IFormFile
    /// </summary>
    public async Task<string> UploadFromFileAsync(IFormFile file, string folder, string fileName, CancellationToken ct = default)
    {
        if (!_isEnabled || _cloudinary == null)
            throw new InvalidOperationException("Cloudinary is not configured");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = fileName,
            Overwrite = true,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {Url}", result.SecureUrl);
        return result.SecureUrl.ToString();
    }

    /// <summary>
    /// Faz upload de uma imagem a partir de bytes
    /// </summary>
    public async Task<string> UploadFromBytesAsync(byte[] bytes, string folder, string fileName, CancellationToken ct = default)
    {
        if (!_isEnabled || _cloudinary == null)
            throw new InvalidOperationException("Cloudinary is not configured");

        using var stream = new MemoryStream(bytes);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = fileName,
            Overwrite = true,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {Url}", result.SecureUrl);
        return result.SecureUrl.ToString();
    }

    /// <summary>
    /// Deleta uma imagem pelo publicId
    /// </summary>
    public async Task<bool> DeleteAsync(string publicId, CancellationToken ct = default)
    {
        if (!_isEnabled || _cloudinary == null)
            return false;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Error}", publicId, result.Error.Message);
            return false;
        }

        _logger.LogInformation("Image deleted from Cloudinary: {PublicId}", publicId);
        return result.Result == "ok";
    }

    /// <summary>
    /// Deleta todas as imagens em uma pasta
    /// </summary>
    public async Task<bool> DeleteFolderAsync(string folder, CancellationToken ct = default)
    {
        if (!_isEnabled || _cloudinary == null)
            return false;

        try
        {
            // Deleta os recursos na pasta
            var deleteParams = new DelResParams
            {
                Prefix = folder,
                Type = "upload"
            };
            await _cloudinary.DeleteResourcesByPrefixAsync(deleteParams.Prefix);

            // Tenta deletar a pasta (pode falhar se não estiver vazia)
            await _cloudinary.DeleteFolderAsync(folder);

            _logger.LogInformation("Folder deleted from Cloudinary: {Folder}", folder);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete Cloudinary folder: {Folder}", folder);
            return false;
        }
    }

    private static byte[] ParseDataUrlToBytes(string dataUrl)
    {
        var s = dataUrl.Trim();
        if (!s.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DataUrl must be an image.");

        var comma = s.IndexOf(',');
        if (comma < 0)
            throw new InvalidOperationException("Invalid DataUrl format.");

        var b64 = s[(comma + 1)..];

        try
        {
            return Convert.FromBase64String(b64);
        }
        catch
        {
            throw new InvalidOperationException("Invalid Base64 in DataUrl.");
        }
    }
}
