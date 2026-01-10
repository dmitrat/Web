using System.Net.Http.Json;
using System.Text.Json;
using OutWit.Web.Framework.Configuration;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Service for loading site configuration.
/// </summary>
public class ConfigService
{
    #region Constructors

    public ConfigService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Get the site configuration, loading it if necessary.
    /// </summary>
    public async Task<SiteConfig> GetConfigAsync()
    {
        if (Config != null)
            return Config;

        try
        {
            Config = await HttpClient.GetFromJsonAsync<SiteConfig>(
                "site.config.json",
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }
        catch (Exception)
        {
            // Return default config if loading fails
            Config = new SiteConfig
            {
                SiteName = "OutWit Web",
                BaseUrl = "/",
                DefaultTheme = "dark"
            };
        }

        return Config!;
    }

    /// <summary>
    /// Reload configuration (useful for development).
    /// </summary>
    public void InvalidateCache()
    {
        Config = null;
    }

    #endregion

    #region Properties

    private HttpClient HttpClient { get; }
    
    private SiteConfig? Config { get; set; }

    #endregion
}
