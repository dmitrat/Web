using System.Text.RegularExpressions;
using Markdig;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Framework.Content;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Generates static HTML pages from markdown content for SEO/SSG.
/// Pre-rendered HTML is visible to crawlers, Blazor hydrates after loading.
/// </summary>
public partial class StaticPageGenerator
{
    #region Fields

    private readonly GeneratorConfig m_config;
    private readonly string m_siteUrl;
    private readonly string m_siteName;
    private readonly MarkdownPipeline m_markdownPipeline;
    private string m_templateHtml = "";

    #endregion

    #region Constructors

    public StaticPageGenerator(GeneratorConfig config, string siteUrl, string siteName)
    {
        m_config = config;
        m_siteUrl = siteUrl.TrimEnd('/');
        m_siteName = siteName;
        
        m_markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    #endregion

    #region Functions

    /// <summary>
    /// Generate static HTML pages for all content.
    /// </summary>
    public async Task GenerateAsync(ContentIndex contentIndex, CancellationToken cancellationToken = default)
    {
        // Load template HTML
        var templatePath = Path.Combine(m_config.OutputPath, "index.html");
        if (!File.Exists(templatePath))
        {
            Console.WriteLine("  Warning: index.html template not found, skipping static page generation");
            return;
        }

        m_templateHtml = await File.ReadAllTextAsync(templatePath, cancellationToken);

        var stats = new GenerationStats();

        // Process blog posts
        await ProcessContentFolderAsync("blog", "blog", contentIndex.Blog, stats, cancellationToken);

        // Process projects
        await ProcessProjectsAsync(contentIndex.Projects, stats, cancellationToken);

        // Process articles
        await ProcessContentFolderAsync("articles", "article", contentIndex.Articles, stats, cancellationToken);

        // Process docs
        await ProcessContentFolderAsync("docs", "docs", contentIndex.Docs, stats, cancellationToken);

        // Process dynamic sections
        foreach (var (sectionName, files) in contentIndex.Sections)
        {
            await ProcessContentFolderAsync(sectionName, sectionName, files, stats, cancellationToken);
            stats.Sections++;
        }

        // Generate index pages for main routes
        await GenerateIndexPagesAsync(stats, cancellationToken);

        Console.WriteLine($"  Generated {stats.Total} static HTML pages:");
        Console.WriteLine($"    Blog: {stats.Blog}, Projects: {stats.Projects}, Articles: {stats.Articles}, Docs: {stats.Docs}, Sections: {stats.SectionItems}, Pages: {stats.Pages}");
    }

    #endregion

    #region Processing Methods

    private async Task ProcessContentFolderAsync(
        string folderName,
        string routePrefix,
        List<string> files,
        GenerationStats stats,
        CancellationToken cancellationToken)
    {
        var folderPath = Path.Combine(m_config.ContentPath, folderName);
        if (!Directory.Exists(folderPath))
            return;

        foreach (var file in files)
        {
            var filePath = Path.Combine(folderPath, file);
            if (!File.Exists(filePath))
                continue;

            try
            {
                var slug = ContentHelpers.GetSlugFromPath(file);
                var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
                var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

                var htmlContent = Markdig.Markdown.ToHtml(content, m_markdownPipeline);
                var pageHtml = GenerateStaticPage(
                    title: frontmatter?.Title ?? slug,
                    description: frontmatter?.Description ?? frontmatter?.Summary ?? "",
                    htmlContent: htmlContent,
                    canonicalUrl: $"{m_siteUrl}/{routePrefix}/{slug}",
                    ogType: "article",
                    publishDate: frontmatter?.PublishDate,
                    tags: frontmatter?.Tags);

                var outputDir = Path.Combine(m_config.OutputPath, routePrefix, slug);
                Directory.CreateDirectory(outputDir);
                await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), pageHtml, cancellationToken);

                IncrementStats(stats, folderName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Warning: Failed to process {file}: {ex.Message}");
            }
        }
    }

    private async Task ProcessProjectsAsync(
        List<string> files,
        GenerationStats stats,
        CancellationToken cancellationToken)
    {
        var projectsPath = Path.Combine(m_config.ContentPath, "projects");
        if (!Directory.Exists(projectsPath))
            return;

        foreach (var file in files)
        {
            var filePath = Path.Combine(projectsPath, file);
            if (!File.Exists(filePath))
                continue;

            try
            {
                var slug = ContentHelpers.GetSlugFromPath(file);
                var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
                var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

                var htmlContent = Markdig.Markdown.ToHtml(content, m_markdownPipeline);
                var pageHtml = GenerateStaticPage(
                    title: frontmatter?.Title ?? slug,
                    description: frontmatter?.Description ?? frontmatter?.Summary ?? "",
                    htmlContent: htmlContent,
                    canonicalUrl: $"{m_siteUrl}/project/{slug}",
                    ogType: "article",
                    tags: frontmatter?.Tags);

                var outputDir = Path.Combine(m_config.OutputPath, "project", slug);
                Directory.CreateDirectory(outputDir);
                await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), pageHtml, cancellationToken);

                stats.Projects++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Warning: Failed to process project {file}: {ex.Message}");
            }
        }
    }

    private async Task GenerateIndexPagesAsync(GenerationStats stats, CancellationToken cancellationToken)
    {
        // Blog list page
        await GenerateIndexPageAsync(
            "Blog",
            "Read the latest articles about software development and more.",
            "blog",
            stats, cancellationToken);

        // Contact page
        await GenerateIndexPageAsync(
            "Contact",
            "Get in touch.",
            "contact",
            stats, cancellationToken);

        // Search page
        await GenerateIndexPageAsync(
            "Search",
            "Search across all content.",
            "search",
            stats, cancellationToken);
    }

    private async Task GenerateIndexPageAsync(
        string title,
        string description,
        string route,
        GenerationStats stats,
        CancellationToken cancellationToken)
    {
        var pageHtml = GenerateStaticPage(
            title: title,
            description: description,
            htmlContent: $"<h1>{title}</h1><p>Loading...</p>",
            canonicalUrl: $"{m_siteUrl}/{route}",
            ogType: "website");

        var outputDir = Path.Combine(m_config.OutputPath, route);
        Directory.CreateDirectory(outputDir);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), pageHtml, cancellationToken);

        stats.Pages++;
    }

    #endregion

    #region Page Generation

    private string GenerateStaticPage(
        string title,
        string description,
        string htmlContent,
        string canonicalUrl,
        string ogType = "website",
        DateTime? publishDate = null,
        List<string>? tags = null)
    {
        var html = m_templateHtml;
        var pageTitle = string.IsNullOrEmpty(title) ? m_siteName : $"{title} - {m_siteName}";
        var metaDescription = EscapeHtmlAttribute(description);

        // Build date HTML
        var dateHtml = "";
        if (publishDate.HasValue && publishDate.Value != default)
        {
            var dateFormatted = publishDate.Value.ToString("MMMM d, yyyy");
            var dateIso = publishDate.Value.ToString("yyyy-MM-dd");
            dateHtml = $"<time datetime=\"{dateIso}\">{dateFormatted}</time>";
        }

        // Build tags HTML
        var tagsHtml = "";
        if (tags is { Count: > 0 })
        {
            tagsHtml = "<div class=\"tags\">" + 
                string.Join("", tags.Select(t => $"<span class=\"tag\">{ContentHelpers.EscapeHtml(t)}</span>")) + 
                "</div>";
        }

        // Build header
        var headerHtml = "";
        if (!string.IsNullOrEmpty(title))
        {
            headerHtml = $"""
                <header class="page-header">
                    <h1 class="page-header__title">{ContentHelpers.EscapeHtml(title)}</h1>
                    {(string.IsNullOrEmpty(dateHtml) ? "" : $"<div class=\"page-header__meta\">{dateHtml}</div>")}
                    {tagsHtml}
                </header>
                """;
        }

        // Build full content
        var fullContent = $"""
            <div class="static-content container">
                {headerHtml}
                <article class="prose">
                    {htmlContent}
                </article>
            </div>
            """;

        // Replace title
        html = TitleRegex().Replace(html, $"<title>{ContentHelpers.EscapeHtml(pageTitle)}</title>");

        // Add/update meta description
        if (MetaDescriptionRegex().IsMatch(html))
        {
            html = MetaDescriptionRegex().Replace(html, $"<meta name=\"description\" content=\"{metaDescription}\" />");
        }
        else
        {
            html = html.Replace("</head>", $"    <meta name=\"description\" content=\"{metaDescription}\" />\n</head>");
        }

        // Build OG image URL (auto-detect based on URL pattern, like PS version)
        var ogImageUrl = GetOgImageUrl(canonicalUrl);

        // Build OG tags
        var ogImageTag = string.IsNullOrEmpty(ogImageUrl) ? "" : $"\n        <meta property=\"og:image\" content=\"{ogImageUrl}\" />";
        var ogTags = $"""
            
                <!-- Open Graph (SSG) -->
                <meta property="og:title" content="{EscapeHtmlAttribute(pageTitle)}" />
                <meta property="og:description" content="{metaDescription}" />
                <meta property="og:type" content="{ogType}" />
                <meta property="og:url" content="{canonicalUrl}" />
                <meta property="og:locale" content="en_US" />{ogImageTag}
                <link rel="canonical" href="{canonicalUrl}" />
            """;

        // Insert OG tags before </head>
        html = html.Replace("</head>", $"{ogTags}\n</head>");

        // Replace app content
        var appContent = $"""
            <div id="app">
                <!-- Static content for SEO - Blazor will hydrate this -->
                {fullContent}
                
                <!-- NoScript fallback -->
                <noscript>
                    <div style="padding: 2rem; text-align: center;">
                        <h1>JavaScript Required</h1>
                        <p>This site requires JavaScript to function properly.</p>
                    </div>
                </noscript>
            </div>
            """;

        html = AppDivRegex().Replace(html, appContent);

        return html;
    }

    #endregion

    #region Tools

    private static void IncrementStats(GenerationStats stats, string folder)
    {
        switch (folder)
        {
            case "blog": stats.Blog++; break;
            case "articles": stats.Articles++; break;
            case "docs": stats.Docs++; break;
            default: stats.SectionItems++; break; // Dynamic sections
        }
    }

    /// <summary>
    /// Generate OG image URL based on canonical URL pattern (like PS version).
    /// </summary>
    private string GetOgImageUrl(string canonicalUrl)
    {
        // Extract path from canonical URL
        var urlPath = canonicalUrl.Replace(m_siteUrl, "").Trim('/');
        
        if (string.IsNullOrEmpty(urlPath))
        {
            // Home page
            return $"{m_siteUrl}/og-images/default.png";
        }

        var segments = urlPath.Split('/');
        if (segments.Length >= 2)
        {
            var contentType = segments[0];
            var slug = segments[1];
            return $"{m_siteUrl}/og-images/{contentType}-{slug}.png";
        }

        return "";
    }

    private static string EscapeHtmlAttribute(string text)
    {
        return ContentHelpers.EscapeHtml(text).Replace("\"", "&quot;");
    }

    #endregion

    #region Regex

    [GeneratedRegex(@"<title>[^<]*</title>")]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<meta\s+name=""description""\s+content=""[^""]*""[^>]*>")]
    private static partial Regex MetaDescriptionRegex();

    [GeneratedRegex(@"<div id=""app"">[\s\S]*?</div>")]
    private static partial Regex AppDivRegex();

    #endregion
}

/// <summary>
/// Statistics for generation progress.
/// </summary>
internal class GenerationStats
{
    public int Blog { get; set; }
    public int Projects { get; set; }
    public int Articles { get; set; }
    public int Docs { get; set; }
    public int Sections { get; set; }
    public int SectionItems { get; set; }
    public int Pages { get; set; }

    public int Total => Blog + Projects + Articles + Docs + SectionItems + Pages;
}
