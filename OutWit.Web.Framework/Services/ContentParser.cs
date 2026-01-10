using System.Text.RegularExpressions;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Universal parser for embedded components in markdown content.
/// Extracts [[Component param="value"]] syntax and converts to structured data.
/// </summary>
public partial class ContentParser
{
    #region Fields

    private int m_placeholderCounter = 0;

    #endregion

    #region Functions

    /// <summary>
    /// Extract all embedded components from content.
    /// Handles both self-closing [[Component /]] and block [[Component]]...[[/Component]] syntax.
    /// </summary>
    public List<EmbeddedComponent> ExtractComponents(string content)
    {
        var components = new List<EmbeddedComponent>();
        m_placeholderCounter = 0;

        // First, extract block components (with closing tag)
        foreach (Match match in BlockComponentRegex().Matches(content))
        {
            var component = ParseComponent(match.Value, match.Index, match.Groups);
            if (component != null)
            {
                component.InnerContent = match.Groups["inner"].Value.Trim();
                components.Add(component);
            }
        }

        // Then extract self-closing components
        foreach (Match match in SelfClosingComponentRegex().Matches(content))
        {
            // Skip if this position was already matched by a block component
            if (components.Any(c => c.Position == match.Index))
                continue;

            var component = ParseComponent(match.Value, match.Index, match.Groups);
            if (component != null)
            {
                components.Add(component);
            }
        }

        return components.OrderBy(c => c.Position).ToList();
    }

    /// <summary>
    /// Replace all component blocks with placeholders.
    /// Returns content with <!--component:id--> placeholders.
    /// </summary>
    public string ReplaceWithPlaceholders(string content, List<EmbeddedComponent> components)
    {
        // Sort by position descending to replace from end to start (preserves positions)
        var sorted = components.OrderByDescending(c => c.Position).ToList();
        
        foreach (var component in sorted)
        {
            content = content.Remove(component.Position, component.OriginalText.Length)
                .Insert(component.Position, $"<!--component:{component.PlaceholderId}-->");
        }

        return content;
    }

    /// <summary>
    /// Parse component from regex match.
    /// </summary>
    private EmbeddedComponent? ParseComponent(string fullMatch, int position, GroupCollection groups)
    {
        var typeName = groups["type"].Value;
        if (string.IsNullOrEmpty(typeName))
            return null;

        var paramsString = groups["params"].Value;
        var parameters = ParseParameters(paramsString);

        return new EmbeddedComponent
        {
            Type = typeName,
            Parameters = parameters,
            Position = position,
            OriginalText = fullMatch,
            PlaceholderId = $"comp_{m_placeholderCounter++}"
        };
    }

    /// <summary>
    /// Parse parameters from string like: param1="value1" param2="value2"
    /// Supports double quotes, single quotes, and unquoted values.
    /// </summary>
    public static Dictionary<string, string> ParseParameters(string paramsString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrWhiteSpace(paramsString))
            return result;

        // Match: name="value" or name='value' or name=value
        var matches = ParameterRegex().Matches(paramsString);
        
        foreach (Match match in matches)
        {
            var name = match.Groups["name"].Value;
            var value = match.Groups["dq"].Success ? match.Groups["dq"].Value :
                match.Groups["sq"].Success ? match.Groups["sq"].Value :
                match.Groups["uq"].Value;
            
            result[name] = value;
        }

        return result;
    }

    /// <summary>
    /// Remove MDX import statements (they're framework-specific).
    /// </summary>
    public string RemoveImportStatements(string content)
    {
        return ImportStatementRegex().Replace(content, "");
    }

    /// <summary>
    /// Full transformation: extract components, replace with placeholders, remove imports.
    /// </summary>
    public (string Content, List<EmbeddedComponent> Components) Transform(string content)
    {
        // Remove imports first
        content = RemoveImportStatements(content);
        
        // Extract components
        var components = ExtractComponents(content);
        
        // Replace with placeholders
        content = ReplaceWithPlaceholders(content, components);
        
        return (content, components);
    }

    #endregion

    #region Regex Patterns

    /// <summary>
    /// Matches block components: [[Type params]]content[[/Type]]
    /// </summary>
    [GeneratedRegex(@"\[\[(?<type>\w+)(?<params>[^\]]*)\]\](?<inner>[\s\S]*?)\[\[/\k<type>\]\]", RegexOptions.IgnoreCase)]
    private static partial Regex BlockComponentRegex();

    /// <summary>
    /// Matches self-closing components: [[Type params]] or [[Type params/]]
    /// </summary>
    [GeneratedRegex(@"\[\[(?<type>\w+)(?<params>[^\]]*?)/??\]\]", RegexOptions.IgnoreCase)]
    private static partial Regex SelfClosingComponentRegex();

    /// <summary>
    /// Matches parameter: name="value" or name='value' or name=value
    /// </summary>
    [GeneratedRegex(@"(?<name>\w+)\s*=\s*(?:""(?<dq>[^""]*)""|'(?<sq>[^']*)'|(?<uq>\S+))", RegexOptions.IgnoreCase)]
    private static partial Regex ParameterRegex();

    /// <summary>
    /// Matches import statements
    /// </summary>
    [GeneratedRegex(@"^\s*import\s+.*?;\s*$", RegexOptions.Multiline)]
    private static partial Regex ImportStatementRegex();

    #endregion
}
