using System.Text;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Configuration;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Generates RSS feed for blog posts.
/// </summary>
public class RssFeedGenerator
{
    #region Constants

    private const int MAX_FEED_ITEMS = 20;

    #endregion

    #region Fields

    private readonly GeneratorConfig m_config;
    private readonly string m_siteUrl;
    private readonly string m_siteName;
    private readonly string m_siteDescription;

    #endregion

    #region Constructors

    public RssFeedGenerator(GeneratorConfig config, string siteUrl, string siteName, string? siteDescription = null)
    {
        m_config = config;
        m_siteUrl = siteUrl.TrimEnd('/');
        m_siteName = siteName;
        m_siteDescription = siteDescription ?? $"Latest posts from {siteName}";
    }

    #endregion

    #region Functions

    /// <summary>
    /// Generate RSS feed from blog posts.
    /// </summary>
    public async Task GenerateAsync(ContentIndex contentIndex, CancellationToken cancellationToken = default)
    {
        var items = new List<RssItem>();

        var blogPath = Path.Combine(m_config.ContentPath, "blog");
        foreach (var file in contentIndex.Blog.Take(MAX_FEED_ITEMS))
        {
            var item = await ParseBlogPostAsync(Path.Combine(blogPath, file), cancellationToken);
            if (item != null)
                items.Add(item);
        }

        var feedPath = Path.Combine(m_config.OutputPath, "feed.xml");
        await WriteFeedAsync(feedPath, items, cancellationToken);
        Console.WriteLine($"  Created: {feedPath} ({items.Count} items)");
    }

    #endregion

    #region Tools

    private async Task<RssItem?> ParseBlogPostAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
            var (frontmatter, _) = ContentHelpers.ExtractFrontmatter(markdown);

            if (frontmatter == null)
                return null;

            var slug = ContentHelpers.GetSlugFromPath(filePath);
            var pubDate = frontmatter.PublishDate != default 
                ? frontmatter.PublishDate 
                : File.GetLastWriteTimeUtc(filePath); // Fallback to file date like PS

            return new RssItem
            {
                Title = frontmatter.Title ?? slug,
                Link = $"{m_siteUrl}/blog/{slug}",
                // Priority: Summary -> Description (like PS version)
                Description = frontmatter.Summary ?? frontmatter.Description ?? "",
                PubDate = pubDate.ToString("R"), // RFC 1123 format
                Guid = $"{m_siteUrl}/blog/{slug}",
                Author = frontmatter.Author ?? ""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Failed to parse blog post {filePath}: {ex.Message}");
            return null;
        }
    }

    private async Task WriteFeedAsync(string path, List<RssItem> items, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
        sb.AppendLine("  <channel>");
        sb.AppendLine($"    <title>{ContentHelpers.EscapeXml(m_siteName)}</title>");
        sb.AppendLine($"    <link>{m_siteUrl}</link>");
        sb.AppendLine($"    <description>{ContentHelpers.EscapeXml(m_siteDescription)}</description>");
        sb.AppendLine("    <language>en-us</language>"); // Added language tag like PS
        sb.AppendLine($"    <lastBuildDate>{DateTime.UtcNow:R}</lastBuildDate>");
        sb.AppendLine($"    <atom:link href=\"{m_siteUrl}/feed.xml\" rel=\"self\" type=\"application/rss+xml\"/>");

        foreach (var item in items)
        {
            sb.AppendLine("    <item>");
            sb.AppendLine($"      <title>{ContentHelpers.EscapeXml(item.Title)}</title>");
            sb.AppendLine($"      <link>{item.Link}</link>");
            sb.AppendLine($"      <description>{ContentHelpers.EscapeXml(item.Description)}</description>");
            sb.AppendLine($"      <pubDate>{item.PubDate}</pubDate>");
            sb.AppendLine($"      <guid isPermaLink=\"true\">{item.Guid}</guid>");
            if (!string.IsNullOrEmpty(item.Author))
            {
                sb.AppendLine($"      <author>{ContentHelpers.EscapeXml(item.Author)}</author>");
            }
            sb.AppendLine("    </item>");
        }

        sb.AppendLine("  </channel>");
        sb.AppendLine("</rss>");

        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }

    #endregion
}

/// <summary>
/// RSS item for feed generation.
/// </summary>
internal class RssItem
{
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    public string Description { get; set; } = "";
    public string PubDate { get; set; } = "";
    public string Guid { get; set; } = "";
    public string Author { get; set; } = "";
}
