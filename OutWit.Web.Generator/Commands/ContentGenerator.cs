using System.Text.Json;
using OutWit.Web.Framework.Content;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Commands;

/// <summary>
/// Main content generator that orchestrates all generation steps.
/// </summary>
public class ContentGenerator
{
    #region Fields

    private readonly GeneratorConfig m_config;
    private readonly ContentScanner m_contentScanner;
    private readonly SiteConfigLoader m_siteConfigLoader;

    #endregion

    #region Constructors

    public ContentGenerator(GeneratorConfig config)
    {
        m_config = config;
        m_contentScanner = new ContentScanner(config.ContentPath, config.SiteConfigPath);
        m_siteConfigLoader = new SiteConfigLoader(config.SiteConfigPath);
    }

    #endregion

    #region Functions

    /// <summary>
    /// Generate all static content based on configuration.
    /// </summary>
    public async Task GenerateAllAsync(CancellationToken cancellationToken = default)
    {
        // Load site configuration
        var siteConfig = await m_siteConfigLoader.LoadAsync(cancellationToken);
        var siteUrl = m_config.SiteUrl ?? siteConfig?.BaseUrl ?? "https://example.com";

        Console.WriteLine($"  Site URL: {siteUrl}");
        Console.WriteLine();

        // Step 1: Scan content folders and generate index.json
        Console.WriteLine("Generating content index...");
        var contentIndex = await m_contentScanner.ScanAsync(cancellationToken);
        await WriteContentIndexAsync(contentIndex, cancellationToken);
        Console.WriteLine($"  Found {contentIndex.Blog.Count} blog posts, {contentIndex.Projects.Count} projects, {contentIndex.Articles.Count} articles, {contentIndex.Docs.Count} docs, {contentIndex.Features.Count} features");

        // Step 2: Generate sitemap.xml and robots.txt
        if (m_config.GenerateSitemap)
        {
            Console.WriteLine("Generating sitemap.xml and robots.txt...");
            var sitemapGenerator = new SitemapGenerator(m_config, siteUrl);
            await sitemapGenerator.GenerateAsync(contentIndex, cancellationToken);
        }

        // Step 3: Generate search index
        if (m_config.GenerateSearchIndex)
        {
            Console.WriteLine("Generating search-index.json...");
            var searchIndexGenerator = new SearchIndexGenerator(m_config);
            await searchIndexGenerator.GenerateAsync(contentIndex, cancellationToken);
        }

        // Step 4: Generate RSS feed
        if (m_config.GenerateRssFeed)
        {
            Console.WriteLine("Generating feed.xml...");
            var rssFeedGenerator = new RssFeedGenerator(m_config, siteUrl, siteConfig?.SiteName ?? "OutWit Web");
            await rssFeedGenerator.GenerateAsync(contentIndex, cancellationToken);
        }

        // Step 5: Generate hosting provider config
        if (m_config.HostingProvider != "none")
        {
            Console.WriteLine($"Generating hosting config for {m_config.HostingProvider}...");
            var hostingConfigGenerator = new HostingConfigGenerator(m_config);
            await hostingConfigGenerator.GenerateAsync(cancellationToken);
        }

        // Step 6: Generate static HTML pages (SSG)
        if (m_config.GenerateStaticPages)
        {
            Console.WriteLine("Generating static HTML pages...");
            var staticPageGenerator = new StaticPageGenerator(m_config, siteUrl, siteConfig?.SiteName ?? "OutWit Web");
            await staticPageGenerator.GenerateAsync(contentIndex, cancellationToken);
        }

        // Step 7: Generate OG images (requires Playwright)
        if (m_config.GenerateOgImages)
        {
            Console.WriteLine("Generating OG images...");
            await using var ogImageGenerator = new OgImageGenerator(m_config, siteUrl, siteConfig?.SiteName ?? "OutWit Web");
            await ogImageGenerator.GenerateAsync(contentIndex, cancellationToken);
        }

        Console.WriteLine();
        Console.WriteLine("Content generation complete!");
    }

    #endregion

    #region Tools

    private async Task WriteContentIndexAsync(ContentIndex contentIndex, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(m_config.ContentPath, "index.json");
        
        // Ensure content directory exists
        Directory.CreateDirectory(m_config.ContentPath);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(contentIndex, options);
        await File.WriteAllTextAsync(indexPath, json, cancellationToken);
        Console.WriteLine($"  Created: {indexPath}");
    }

    #endregion
}
