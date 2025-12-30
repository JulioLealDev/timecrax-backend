namespace Timecrax.Api.Services;

/// <summary>
/// Configuration options for file storage.
/// Binds to the "Storage" section in appsettings.json.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Root path for file storage on disk.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Public base URL for accessing stored files.
    /// </summary>
    public string PublicBasePath { get; set; } = string.Empty;

    /// <summary>
    /// Validates that required configuration values are set.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(RootPath) && !string.IsNullOrWhiteSpace(PublicBasePath);

    /// <summary>
    /// Gets the public base path without trailing slash.
    /// </summary>
    public string PublicBasePathTrimmed => PublicBasePath.TrimEnd('/');
}
