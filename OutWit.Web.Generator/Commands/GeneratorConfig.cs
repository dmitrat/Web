namespace OutWit.Web.Generator.Commands;

/// <summary>
/// Configuration for the content generator.
/// </summary>
public class GeneratorConfig
{
    #region Properties

    /// <summary>
    /// Path to the site project directory.
    /// </summary>
    public string SitePath { get; set; } = "";

    /// <summary>
    /// Output directory for generated files (typically wwwroot).
    /// </summary>
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// Site URL for generating absolute links.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Whether to generate sitemap.xml and robots.txt.
    /// </summary>
    public bool GenerateSitemap { get; set; } = true;

    /// <summary>
    /// Whether to generate search-index.json.
    /// </summary>
    public bool GenerateSearchIndex { get; set; } = true;

    /// <summary>
    /// Whether to generate RSS feed.
    /// </summary>
    public bool GenerateRssFeed { get; set; } = true;

    /// <summary>
    /// Whether to generate static HTML pages.
    /// </summary>
    public bool GenerateStaticPages { get; set; } = true;

    /// <summary>
    /// Whether to generate OG images using Playwright.
    /// </summary>
    public bool GenerateOgImages { get; set; } = false;

    /// <summary>
    /// Hosting provider for config file generation.
    /// </summary>
    public string HostingProvider { get; set; } = "cloudflare";

    /// <summary>
    /// Maximum content length for search index entries.
    /// </summary>
    public int SearchContentMaxLength { get; set; } = 10000;

    /// <summary>
    /// Force regeneration of OG images even if they exist.
    /// </summary>
    public bool ForceOgImages { get; set; } = false;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Path to the content folder.
    /// </summary>
    public string ContentPath => Path.Combine(OutputPath, "content");

    /// <summary>
    /// Path to site.config.json.
    /// </summary>
    public string SiteConfigPath => Path.Combine(OutputPath, "site.config.json");

    #endregion
}
