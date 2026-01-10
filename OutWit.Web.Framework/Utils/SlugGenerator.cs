using System.Text.RegularExpressions;

namespace OutWit.Web.Framework.Utils;

/// <summary>
/// Utility class for generating URL-safe slugs from text.
/// Matches Markdig's AutoIdentifiers extension algorithm.
/// </summary>
public static partial class SlugGenerator
{
    /// <summary>
    /// Generate a URL-safe slug from text.
    /// Matches Markdig's AutoIdentifiers extension algorithm.
    /// </summary>
    /// <param name="text">Text to convert to slug</param>
    /// <returns>URL-safe slug</returns>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
            
        // Markdig's AutoIdentifiers: lowercase, replace spaces with hyphens, 
        // keep alphanumeric, hyphens, and dots (for .NET etc.)
        var slug = text.ToLowerInvariant();
        
        // Replace spaces and underscores with hyphens
        slug = WhitespaceRegex().Replace(slug, "-");
        
        // Remove non-alphanumeric characters except hyphens and dots
        slug = InvalidCharsRegex().Replace(slug, "");
        
        // Collapse multiple hyphens
        slug = MultipleDashesRegex().Replace(slug, "-");
        
        // Remove hyphens before and after dots (Markdig produces "vs.native" not "vs.-native")
        slug = slug.Replace("-.", ".");
        slug = slug.Replace(".-", ".");
        
        return slug.Trim('-');
    }

    /// <summary>
    /// Get slug from a filename, handling date and order prefixes.
    /// </summary>
    /// <param name="filename">Filename like "2024-01-15-my-post.md" or "01-biography.md"</param>
    /// <returns>Slug like "my-post" or "biography"</returns>
    public static string GetSlugFromFilename(string filename)
    {
        var name = filename;
        
        // Handle folder-based content: "01-biography/index.md" -> use folder name
        if (name.Contains('/'))
        {
            var parts = name.Split('/');
            // If it's a folder with index.md, use the folder name
            if (parts.Length >= 2 && parts[^1].StartsWith("index.", StringComparison.OrdinalIgnoreCase))
            {
                name = parts[^2]; // Use folder name (e.g., "01-biography")
            }
            else
            {
                name = parts[^1]; // Use filename
            }
        }
        
        // Remove extension
        if (name.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        else if (name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            name = name[..^3];

        // Remove date prefix if present (e.g., 2024-11-20-title.md)
        if (name.Length > 11 && char.IsDigit(name[0]) && name[4] == '-' && name[7] == '-' && name[10] == '-')
            name = name[11..];
        // Remove order prefix if present (e.g., 01-title.md)
        else if (name.Length > 3 && char.IsDigit(name[0]) && char.IsDigit(name[1]) && name[2] == '-')
            name = name[3..];

        return name;
    }

    /// <summary>
    /// Get order number and slug from a filename with order prefix.
    /// </summary>
    /// <param name="filename">Filename like "01-biography/index.md"</param>
    /// <returns>Tuple of (order, slug) like (1, "biography")</returns>
    public static (int Order, string Slug) GetOrderAndSlugFromFilename(string filename)
    {
        var name = filename;
        
        // Handle folder-based content: "01-biography/index.md" -> use folder name
        if (name.Contains('/'))
        {
            var parts = name.Split('/');
            // If it's a folder with index.md, use the folder name
            if (parts.Length >= 2 && parts[^1].StartsWith("index.", StringComparison.OrdinalIgnoreCase))
            {
                name = parts[^2]; // Use folder name (e.g., "01-biography")
            }
            else
            {
                name = parts[^1]; // Use filename
            }
        }
        
        // Remove extension
        if (name.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        else if (name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            name = name[..^3];

        int order = 0;
        string slug = name;

        // Extract order prefix (e.g., "01-biography" -> order=1, slug="biography")
        if (name.Length > 3 && char.IsDigit(name[0]) && char.IsDigit(name[1]) && name[2] == '-')
        {
            order = int.TryParse(name[..2], out var o) ? o : 0;
            slug = name[3..];
        }

        return (order, slug);
    }

    #region Regex Patterns

    [GeneratedRegex(@"[\s_]+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-\.]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleDashesRegex();

    #endregion
}
