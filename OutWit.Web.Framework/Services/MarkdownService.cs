using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Utils;
using System.Text.RegularExpressions;
using Markdig.Extensions.Yaml;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Service for parsing and rendering Markdown content with frontmatter support.
/// </summary>
public partial class MarkdownService
{
    #region Constructors

    public MarkdownService()
    {
        Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UseAutoIdentifiers()
            .UseTaskLists()
            .UseEmojiAndSmiley()
            .Build();

        YamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    #endregion

    #region Functions

    /// <summary>
    /// Parse markdown content and extract frontmatter.
    /// </summary>
    public (T? Frontmatter, string HtmlContent) ParseWithFrontmatter<T>(string markdown) where T : class
    {
        var document = Markdown.Parse(markdown, Pipeline);
        
        // Extract YAML frontmatter
        T? frontmatter = default;
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        
        if (yamlBlock != null)
        {
            var yamlContent = markdown.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);
            // Remove the --- delimiters
            yamlContent = YamlFrontMatterRegex().Replace(yamlContent, "").Trim();
            
            try
            {
                frontmatter = YamlDeserializer.Deserialize<T>(yamlContent);
            }
            catch (Exception)
            {
                // Log or handle YAML parsing error
            }
        }

        // Render HTML
        var html = Markdown.ToHtml(markdown, Pipeline);
        
        return (frontmatter, html);
    }

    /// <summary>
    /// Render markdown to HTML.
    /// </summary>
    public string ToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown, Pipeline);
    }

    /// <summary>
    /// Extract table of contents from markdown.
    /// </summary>
    public List<TocEntry> ExtractTableOfContents(string markdown)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        var entries = new List<TocEntry>();
        var stack = new Stack<TocEntry>();

        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            var text = GetHeadingText(heading);
            var id = SlugGenerator.GenerateSlug(text);
            var entry = new TocEntry
            {
                Id = id,
                Title = text,
                Level = heading.Level
            };

            // Build hierarchy
            while (stack.Count > 0 && stack.Peek().Level >= heading.Level)
            {
                stack.Pop();
            }

            if (stack.Count == 0)
            {
                entries.Add(entry);
            }
            else
            {
                stack.Peek().Children.Add(entry);
            }

            stack.Push(entry);
        }

        return entries;
    }

    /// <summary>
    /// Calculate reading time in minutes.
    /// </summary>
    public int CalculateReadingTime(string markdown, int wordsPerMinute = 200)
    {
        // Remove markdown syntax for more accurate word count
        var plainText = StripMarkdownRegex().Replace(markdown, " ");
        var wordCount = plainText.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling((double)wordCount / wordsPerMinute));
    }

    /// <summary>
    /// Extract plain text from markdown for search indexing.
    /// </summary>
    public string ExtractPlainText(string markdown)
    {
        // Remove YAML frontmatter
        var content = YamlFrontMatterBlockRegex().Replace(markdown, "");
        // Remove markdown syntax
        content = StripMarkdownRegex().Replace(content, " ");
        // Normalize whitespace
        content = MultipleSpacesRegex().Replace(content, " ").Trim();
        return content;
    }

    #endregion

    #region Tools

    private static string GetHeadingText(HeadingBlock heading)
    {
        var inline = heading.Inline;
        if (inline == null) return string.Empty;
        
        return string.Concat(inline.Descendants<LiteralInline>().Select(l => l.Content.ToString()));
    }

    #endregion

    #region Properties

    private MarkdownPipeline Pipeline { get; }
    
    private IDeserializer YamlDeserializer { get; }

    #endregion

    #region Regex Patterns

    [GeneratedRegex(@"^---\s*\n?|---\s*$", RegexOptions.Multiline)]
    private static partial Regex YamlFrontMatterRegex();

    [GeneratedRegex(@"^---[\s\S]*?---\s*", RegexOptions.Multiline)]
    private static partial Regex YamlFrontMatterBlockRegex();

    [GeneratedRegex(@"[#*`\[\]()>!_~\-]")]
    private static partial Regex StripMarkdownRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();

    #endregion
}
