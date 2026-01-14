using System.Text.Json;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Configuration;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Scans content folders and builds the content index.
/// Supports both hardcoded sections (blog, projects, features) and
/// dynamic sections defined in site.config.json.
/// </summary>
public class ContentScanner
{
    #region Fields

    private readonly string m_contentPath;
    private readonly string m_siteConfigPath;

    #endregion

    #region Constructors

    public ContentScanner(string contentPath, string? siteConfigPath = null)
    {
        m_contentPath = contentPath;
        m_siteConfigPath = siteConfigPath ?? Path.Combine(Path.GetDirectoryName(contentPath) ?? "", "site.config.json");
    }

    #endregion

    #region Functions

    /// <summary>
    /// Scan all content folders and build the index.
    /// </summary>
    public async Task<ContentIndex> ScanAsync(CancellationToken cancellationToken = default)
    {
        var index = new ContentIndex();

        // Scan hardcoded sections
        
        // Blog posts (sorted by name descending for date-based posts)
        var blogPath = Path.Combine(m_contentPath, "blog");
        if (Directory.Exists(blogPath))
        {
            index.Blog = ScanFolder(blogPath, "*.md*", sortDescending: true);
        }

        // Projects (special folder structure)
        var projectsPath = Path.Combine(m_contentPath, "projects");
        if (Directory.Exists(projectsPath))
        {
            index.Projects = ScanProjectsFolder(projectsPath);
        }

        // Features (sorted by order prefix)
        var featuresPath = Path.Combine(m_contentPath, "features");
        if (Directory.Exists(featuresPath))
        {
            index.Features = ScanFolder(featuresPath, "*.md*", sortDescending: false);
        }

        // Legacy hardcoded sections (for backward compatibility)
        var articlesPath = Path.Combine(m_contentPath, "articles");
        if (Directory.Exists(articlesPath))
        {
            index.Articles = ScanFolder(articlesPath, "*.md*", sortDescending: false);
        }

        var docsPath = Path.Combine(m_contentPath, "docs");
        if (Directory.Exists(docsPath))
        {
            index.Docs = ScanFolder(docsPath, "*.md*", sortDescending: false);
        }

        // Scan dynamic sections from site.config.json
        var siteConfig = await LoadSiteConfigAsync(cancellationToken);
        if (siteConfig?.ContentSections != null)
        {
            foreach (var section in siteConfig.ContentSections)
            {
                // Skip if folder matches a hardcoded section
                if (IsHardcodedSection(section.Folder))
                    continue;

                var sectionPath = Path.Combine(m_contentPath, section.Folder);
                if (Directory.Exists(sectionPath))
                {
                    index.Sections[section.Folder] = ScanFolder(sectionPath, "*.md*", sortDescending: false);
                }
            }
        }

        return index;
    }

    #endregion

    #region Tools

    private static bool IsHardcodedSection(string folder)
    {
        return folder.Equals("blog", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("projects", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("features", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("articles", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("docs", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<SiteConfig?> LoadSiteConfigAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(m_siteConfigPath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(m_siteConfigPath, cancellationToken);
            return JsonSerializer.Deserialize<SiteConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static List<string> ScanFolder(string path, string pattern, bool sortDescending)
    {
        var files = Directory.GetFiles(path, pattern)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();

        return sortDescending
            ? files.OrderByDescending(f => f).ToList()
            : files.OrderBy(f => f).ToList();
    }

    private static List<string> ScanProjectsFolder(string path)
    {
        var results = new List<string>();

        foreach (var item in Directory.GetFileSystemEntries(path).OrderBy(p => p))
        {
            var name = Path.GetFileName(item);
            
            if (Directory.Exists(item))
            {
                // Folder-based project: look for index.md
                var indexFile = Path.Combine(item, "index.md");
                if (File.Exists(indexFile))
                {
                    results.Add($"{name}/index.md");
                }
            }
            else if (item.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                // File-based project
                results.Add(name);
            }
        }

        return results;
    }

    #endregion
}
