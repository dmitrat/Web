using System.Text.Json;
using OutWit.Web.Framework.Configuration;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Loads site configuration from site.config.json.
/// </summary>
public class SiteConfigLoader
{
    #region Fields

    private readonly string m_configPath;

    #endregion

    #region Constructors

    public SiteConfigLoader(string configPath)
    {
        m_configPath = configPath;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Load site configuration from JSON file.
    /// </summary>
    public async Task<SiteConfig?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(m_configPath))
        {
            Console.WriteLine($"  Warning: site.config.json not found at {m_configPath}");
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(m_configPath, cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<SiteConfig>(json, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Failed to parse site.config.json: {ex.Message}");
            return null;
        }
    }

    #endregion
}
