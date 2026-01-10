using System.Net.Http.Json;
using System.Text.Json;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Service for client-side full-text search functionality.
/// Loads pre-built search index from search-index.json (generated at build time).
/// Falls back to building index on client if pre-built index is not available.
/// </summary>
public class SearchService
{
    #region Fields

    private List<SearchIndexEntry>? m_searchIndex;
    
    private readonly SemaphoreSlim m_indexLock = new(1, 1);

    #endregion

    #region Constructors

    public SearchService(HttpClient httpClient, ContentService contentService, MarkdownService markdownService)
    {
        HttpClient = httpClient;
        ContentService = contentService;
        MarkdownService = markdownService;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Get or load the search index.
    /// First tries to load pre-built index, falls back to building on client.
    /// </summary>
    public async Task<List<SearchIndexEntry>> GetSearchIndexAsync()
    {
        if (m_searchIndex != null)
            return m_searchIndex;

        await m_indexLock.WaitAsync();
        try
        {
            if (m_searchIndex != null)
                return m_searchIndex;

            // Try to load pre-built index first
            m_searchIndex = await LoadPreBuiltIndexAsync();
            
            // Fall back to building on client if pre-built not available
            if (m_searchIndex == null || m_searchIndex.Count == 0)
            {
                Console.WriteLine("Pre-built search index not found, building on client...");
                m_searchIndex = await BuildSearchIndexAsync();
            }
            
            return m_searchIndex;
        }
        finally
        {
            m_indexLock.Release();
        }
    }

    /// <summary>
    /// Load pre-built search index from search-index.json.
    /// </summary>
    private async Task<List<SearchIndexEntry>?> LoadPreBuiltIndexAsync()
    {
        try
        {
            var index = await HttpClient.GetFromJsonAsync<List<SearchIndexEntry>>(
                "search-index.json",
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
            
            if (index != null && index.Count > 0)
            {
                Console.WriteLine($"Loaded pre-built search index with {index.Count} entries");
            }
            
            return index;
        }
        catch (HttpRequestException)
        {
            // File not found - pre-built index not available
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading search index: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Build the search index from all content types (fallback for development).
    /// </summary>
    private async Task<List<SearchIndexEntry>> BuildSearchIndexAsync()
    {
        var entries = new List<SearchIndexEntry>();

        // Add projects
        var projects = await ContentService.GetProjectsAsync();
        foreach (var project in projects)
        {
            entries.Add(new SearchIndexEntry
            {
                Title = project.Title,
                Description = project.Summary ?? "",
                Content = TruncateContent(MarkdownService.ExtractPlainText(project.HtmlContent)),
                Url = $"/project/{project.Slug}",
                Type = "project",
                Tags = project.Tags
            });
        }

        // Add blog posts
        var posts = await ContentService.GetBlogPostsAsync();
        foreach (var post in posts)
        {
            entries.Add(new SearchIndexEntry
            {
                Title = post.Title,
                Description = post.Summary ?? post.Description ?? "",
                Content = TruncateContent(MarkdownService.ExtractPlainText(post.HtmlContent)),
                Url = $"/blog/{post.Slug}",
                Type = "blog",
                Tags = post.Tags
            });
        }

        // Add articles
        var articles = await ContentService.GetArticlesAsync();
        foreach (var article in articles)
        {
            entries.Add(new SearchIndexEntry
            {
                Title = article.Title,
                Description = article.Description ?? "",
                Content = TruncateContent(MarkdownService.ExtractPlainText(article.HtmlContent)),
                Url = $"/article/{article.Slug}",
                Type = "article",
                Tags = article.Tags
            });
        }

        return entries;
    }

    /// <summary>
    /// Simple C# search fallback (for when JS is not available).
    /// Returns entries matching the query in title, description, content, or tags.
    /// </summary>
    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SearchResult>();

        var index = await GetSearchIndexAsync();
        var queryLower = query.ToLowerInvariant();
        var terms = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var results = new List<SearchResult>();

        foreach (var entry in index)
        {
            var score = CalculateScore(entry, terms);
            if (score > 0)
            {
                results.Add(new SearchResult
                {
                    Title = entry.Title,
                    Description = GetExcerpt(entry.Description, entry.Content, terms),
                    Url = entry.Url,
                    ContentType = entry.Type,
                    Score = score,
                    MatchedTerms = terms.ToList()
                });
            }
        }

        return results.OrderByDescending(r => r.Score).Take(20).ToList();
    }
    
    /// <summary>
    /// Export search index as JSON for client-side Fuse.js.
    /// </summary>
    public async Task<string> ExportIndexAsJsonAsync()
    {
        var index = await GetSearchIndexAsync();
        return JsonSerializer.Serialize(index, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    #endregion

    #region Tools

    /// <summary>
    /// Truncate content to reasonable size for search index.
    /// </summary>
    private static string TruncateContent(string content, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;
        return content[..maxLength];
    }

    /// <summary>
    /// Calculate relevance score for an entry.
    /// </summary>
    private static double CalculateScore(SearchIndexEntry entry, string[] terms)
    {
        double score = 0;
        var titleLower = entry.Title.ToLowerInvariant();
        var descLower = entry.Description.ToLowerInvariant();
        var contentLower = entry.Content.ToLowerInvariant();
        var tagsLower = string.Join(" ", entry.Tags).ToLowerInvariant();

        foreach (var term in terms)
        {
            // Title matches are most important
            if (titleLower.Contains(term))
                score += 10;
            
            // Tag matches are important
            if (tagsLower.Contains(term))
                score += 5;
            
            // Description matches
            if (descLower.Contains(term))
                score += 3;
            
            // Content matches
            if (contentLower.Contains(term))
                score += 1;
        }

        return score;
    }

    /// <summary>
    /// Get an excerpt with context around the first match.
    /// </summary>
    private static string GetExcerpt(string description, string content, string[] terms)
    {
        // Prefer description if it contains a match
        var descLower = description.ToLowerInvariant();
        foreach (var term in terms)
        {
            if (descLower.Contains(term))
                return TruncateWithEllipsis(description, 200);
        }

        // Otherwise find first match in content
        var contentLower = content.ToLowerInvariant();
        foreach (var term in terms)
        {
            var idx = contentLower.IndexOf(term);
            if (idx >= 0)
            {
                var start = Math.Max(0, idx - 50);
                var length = Math.Min(200, content.Length - start);
                var excerpt = content.Substring(start, length);
                if (start > 0) excerpt = "..." + excerpt;
                if (start + length < content.Length) excerpt += "...";
                return excerpt;
            }
        }

        // Fallback to description or truncated content
        return !string.IsNullOrEmpty(description) 
            ? TruncateWithEllipsis(description, 200) 
            : TruncateWithEllipsis(content, 200);
    }

    private static string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text[..maxLength] + "...";
    }

    #endregion

    #region Properties

    private HttpClient HttpClient { get; }
    
    private ContentService ContentService { get; }
    
    private MarkdownService MarkdownService { get; }

    #endregion
}
