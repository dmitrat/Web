using System.Text.Json;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Framework.Content;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Generates pre-built search index JSON file.
/// </summary>
public class SearchIndexGenerator
{
    #region Fields

    private readonly GeneratorConfig m_config;

    #endregion

    #region Constructors

    public SearchIndexGenerator(GeneratorConfig config)
    {
        m_config = config;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Generate search-index.json from content files.
    /// </summary>
    public async Task GenerateAsync(ContentIndex contentIndex, CancellationToken cancellationToken = default)
    {
        var entries = new List<SearchIndexEntry>();

        // Index blog posts
        var blogPath = Path.Combine(m_config.ContentPath, "blog");
        foreach (var file in contentIndex.Blog)
        {
            var entry = await ParseContentAsync(Path.Combine(blogPath, file), "blog", cancellationToken);
            if (entry != null)
                entries.Add(entry);
        }

        // Index projects
        var projectsPath = Path.Combine(m_config.ContentPath, "projects");
        foreach (var file in contentIndex.Projects)
        {
            var entry = await ParseContentAsync(Path.Combine(projectsPath, file), "project", cancellationToken);
            if (entry != null)
                entries.Add(entry);
        }

        // Index articles
        var articlesPath = Path.Combine(m_config.ContentPath, "articles");
        foreach (var file in contentIndex.Articles)
        {
            var entry = await ParseContentAsync(Path.Combine(articlesPath, file), "article", cancellationToken);
            if (entry != null)
                entries.Add(entry);
        }

        // Index docs
        var docsPath = Path.Combine(m_config.ContentPath, "docs");
        foreach (var file in contentIndex.Docs)
        {
            var entry = await ParseContentAsync(Path.Combine(docsPath, file), "docs", cancellationToken);
            if (entry != null)
                entries.Add(entry);
        }

        // Index dynamic sections
        foreach (var (sectionName, files) in contentIndex.Sections)
        {
            var sectionPath = Path.Combine(m_config.ContentPath, sectionName);
            foreach (var file in files)
            {
                var entry = await ParseContentAsync(Path.Combine(sectionPath, file), sectionName, cancellationToken);
                if (entry != null)
                    entries.Add(entry);
            }
        }

        // Write search index (compressed like PS version for smaller file size)
        var indexPath = Path.Combine(m_config.OutputPath, "search-index.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = false, // Compressed output like PS version
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(entries, options);
        await File.WriteAllTextAsync(indexPath, json, cancellationToken);
        
        var sizekB = Math.Round(json.Length / 1024.0, 1);
        Console.WriteLine($"  Created: {indexPath} ({entries.Count} entries, {sizekB} KB)");
    }

    #endregion

    #region Tools

    private async Task<SearchIndexEntry?> ParseContentAsync(string filePath, string contentType, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
            var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

            if (frontmatter == null)
                return null;

            var slug = ContentHelpers.GetSlugFromPath(filePath);
            var plainText = ContentHelpers.ExtractPlainText(content);

            // Use configurable max length (default 10000 like PS version)
            var maxLength = m_config.SearchContentMaxLength;

            return new SearchIndexEntry
            {
                Title = frontmatter.Title ?? slug,
                Description = frontmatter.Summary ?? frontmatter.Description ?? "",
                Content = ContentHelpers.TruncateText(plainText, maxLength),
                Url = $"/{contentType}/{slug}",
                Type = contentType,
                Tags = frontmatter.Tags ?? []
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Failed to parse {filePath}: {ex.Message}");
            return null;
        }
    }

    #endregion
}

/// <summary>
/// Search index entry for JSON serialization.
/// </summary>
public class SearchIndexEntry
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Content { get; set; } = "";
    public string Url { get; set; } = "";
    public string Type { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}
