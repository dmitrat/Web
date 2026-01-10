using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Represents an embedded component extracted from markdown content.
/// Syntax: [[ComponentName param1="value1" param2="value2"]]
/// Or with content: [[ComponentName param="value"]] inner content [[/ComponentName]]
/// </summary>
public class EmbeddedComponent : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not EmbeddedComponent other)
            return false;
        
        return Type.Is(other.Type)
            && Parameters.Is(other.Parameters)
            && InnerContent.Is(other.InnerContent)
            && Position.Is(other.Position)
            && OriginalText.Is(other.OriginalText)
            && PlaceholderId.Is(other.PlaceholderId)
            && BasePath.Is(other.BasePath);
    }

    public override EmbeddedComponent Clone()
    {
        return new EmbeddedComponent
        {
            Type = Type,
            Parameters = Parameters.ToDictionary(pair => pair.Key, pair => pair.Value),
            InnerContent = InnerContent,
            Position = Position,
            OriginalText = OriginalText,
            PlaceholderId = PlaceholderId,
            BasePath = BasePath
        };
    }

    #endregion

    #region Properties

    /// <summary>Component type name (e.g., "YouTube", "FloatingImage", "PowerPoint")</summary>
    [ToString]
    public string Type { get; set; } = "";
    
    /// <summary>Parameters passed to the component</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    /// <summary>Inner content for wrapper components (content between [[Component]] and [[/Component]])</summary>
    public string? InnerContent { get; set; }
    
    /// <summary>Position in the original content (for ordering)</summary>
    public int Position { get; set; }
    
    /// <summary>Original matched text (for replacement)</summary>
    public string OriginalText { get; set; } = "";
    
    /// <summary>Unique placeholder ID for this component</summary>
    public string PlaceholderId { get; set; } = "";
    
    /// <summary>Base path for resolving relative URLs (e.g., "content/projects/01-biography")</summary>
    public string BasePath { get; set; } = "";

    #endregion
}