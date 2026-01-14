using CommandLine;

namespace OutWit.Web.Generator.Commands;

/// <summary>
/// Command-line options for the content generator.
/// </summary>
public class GeneratorOptions
{
    #region Properties

    /// <summary>
    /// Path to the site project directory.
    /// </summary>
    [Option('s', "site", Required = true, HelpText = "Path to the site project directory.")]
    public string SitePath { get; set; } = "";

    /// <summary>
    /// Output directory (defaults to site/wwwroot).
    /// </summary>
    [Option('o', "output", Required = false, HelpText = "Output directory (defaults to site/wwwroot).")]
    public string? OutputPath { get; set; }

    /// <summary>
    /// Site URL for generating absolute links.
    /// </summary>
    [Option('u', "url", Required = false, HelpText = "Site URL (reads from site.config.json if not specified).")]
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Skip sitemap.xml and robots.txt generation.
    /// </summary>
    [Option("skip-sitemap", Required = false, Default = false, HelpText = "Skip sitemap.xml and robots.txt generation.")]
    public bool SkipSitemap { get; set; }

    /// <summary>
    /// Skip search-index.json generation.
    /// </summary>
    [Option("skip-search", Required = false, Default = false, HelpText = "Skip search-index.json generation.")]
    public bool SkipSearch { get; set; }

    /// <summary>
    /// Skip RSS feed generation.
    /// </summary>
    [Option("skip-rss", Required = false, Default = false, HelpText = "Skip RSS feed generation.")]
    public bool SkipRss { get; set; }

    /// <summary>
    /// Skip static HTML pages generation.
    /// </summary>
    [Option("skip-static", Required = false, Default = false, HelpText = "Skip static HTML pages generation.")]
    public bool SkipStatic { get; set; }

    /// <summary>
    /// Generate OG images using Playwright (requires Playwright browser).
    /// </summary>
    [Option("og-images", Required = false, Default = false, HelpText = "Generate OG images using Playwright.")]
    public bool GenerateOgImages { get; set; }

    /// <summary>
    /// Force regeneration of OG images even if they exist.
    /// </summary>
    [Option("force-og", Required = false, Default = false, HelpText = "Force regeneration of OG images even if they exist.")]
    public bool ForceOgImages { get; set; }

    /// <summary>
    /// Maximum content length for search index entries.
    /// </summary>
    [Option("search-content-max-length", Required = false, Default = 10000, HelpText = "Maximum content length for search index entries (default: 10000).")]
    public int SearchContentMaxLength { get; set; } = 10000;

    /// <summary>
    /// Hosting provider for config file generation.
    /// </summary>
    [Option('h', "hosting", Required = false, Default = "cloudflare", HelpText = "Hosting provider (cloudflare, netlify, vercel, github, none).")]
    public string HostingProvider { get; set; } = "cloudflare";

    #endregion

    #region Functions

    /// <summary>
    /// Get the output path, defaulting to site/wwwroot if not specified.
    /// </summary>
    public string GetOutputPath()
    {
        return OutputPath ?? Path.Combine(SitePath, "wwwroot");
    }

    #endregion
}
