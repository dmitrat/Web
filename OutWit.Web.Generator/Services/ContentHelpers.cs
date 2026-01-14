using System.Text.RegularExpressions;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Common helper methods for content processing.
/// Delegates to Framework utilities where possible to avoid duplication.
/// </summary>
public static partial class ContentHelpers
{
    #region Constants

    private const int DEFAULT_MAX_TEXT_LENGTH = 500;

    #endregion

    #region Fields

    private static readonly IDeserializer s_yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    #endregion

    #region Functions

    /// <summary>
    /// Extract frontmatter from markdown content.
    /// Returns raw markdown content (not HTML) for generator use cases.
    /// </summary>
    /// <param name="markdown">The markdown content with YAML frontmatter.</param>
    /// <returns>Tuple of parsed frontmatter and remaining raw content.</returns>
    public static (FrontmatterData?, string) ExtractFrontmatter(string markdown)
    {
        var match = FrontmatterRegex().Match(markdown);
        if (!match.Success)
            return (null, markdown);

        var yaml = match.Groups[1].Value;
        var content = markdown[match.Length..].Trim();

        try
        {
            var frontmatter = s_yamlDeserializer.Deserialize<FrontmatterData>(yaml);
            return (frontmatter, content);
        }
        catch
        {
            return (null, markdown);
        }
    }

    /// <summary>
    /// Get slug from file path. Extracts filename and delegates to Framework SlugGenerator.
    /// Handles both full paths and relative filenames.
    /// </summary>
    /// <param name="path">Full path or filename.</param>
    /// <returns>Clean slug for URL generation.</returns>
    public static string GetSlugFromPath(string path)
    {
        // Normalize path separators for cross-platform support
        var normalized = path.Replace('\\', '/');
        
        // Handle folder-based content (e.g., "projects/01-biography/index.md")
        if (normalized.EndsWith("/index.md", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith("/index.mdx", StringComparison.OrdinalIgnoreCase))
        {
            // Get parent folder name
            var parts = normalized.Split('/');
            if (parts.Length >= 2)
            {
                var folderName = parts[^2];
                return SlugGenerator.GetSlugFromFilename(folderName);
            }
        }
        
        // Extract just the filename from full path
        var filename = Path.GetFileName(path);
        
        // Use Framework utility
        return SlugGenerator.GetSlugFromFilename(filename);
    }

    /// <summary>
    /// Extract plain text from markdown content.
    /// Removes markdown syntax for search indexing and descriptions.
    /// </summary>
    /// <param name="markdown">Markdown content.</param>
    /// <returns>Plain text with markdown syntax removed.</returns>
    public static string ExtractPlainText(string markdown)
    {
        // Remove frontmatter first
        var content = FrontmatterBlockRegex().Replace(markdown, "");
        // Remove markdown syntax
        content = MarkdownSyntaxRegex().Replace(content, " ");
        // Normalize whitespace
        content = MultipleSpacesRegex().Replace(content, " ");
        return content.Trim();
    }

    /// <summary>
    /// Truncate text to specified maximum length.
    /// </summary>
    /// <param name="content">Text to truncate.</param>
    /// <param name="maxLength">Maximum length.</param>
    /// <returns>Truncated text with ellipsis if needed.</returns>
    public static string TruncateText(string content, int maxLength = DEFAULT_MAX_TEXT_LENGTH)
    {
        if (content.Length <= maxLength)
            return content;

        return content[..maxLength] + "...";
    }

    /// <summary>
    /// Escape special characters for XML content.
    /// </summary>
    /// <param name="text">Text to escape.</param>
    /// <returns>XML-safe text.</returns>
    public static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    /// <summary>
    /// Escape special characters for HTML content.
    /// </summary>
    /// <param name="text">Text to escape.</param>
    /// <returns>HTML-safe text.</returns>
    public static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    #endregion

    #region Regex

    [GeneratedRegex(@"^---\s*\n([\s\S]*?)\n---\s*\n?", RegexOptions.Compiled)]
    public static partial Regex FrontmatterRegex();

    [GeneratedRegex(@"^---[\s\S]*?---\s*", RegexOptions.Multiline)]
    private static partial Regex FrontmatterBlockRegex();

    [GeneratedRegex(@"[#*`\[\]()>!_~\-]")]
    private static partial Regex MarkdownSyntaxRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();

    #endregion
}
