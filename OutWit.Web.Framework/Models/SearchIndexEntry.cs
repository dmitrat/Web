using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Search index entry for Fuse.js.
/// </summary>
public class SearchIndexEntry : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not SearchIndexEntry other)
            return false;

        return Title.Is(other.Title)
               && Description.Is(other.Description)
               && Content.Is(other.Content)
               && Url.Is(other.Url)
               && Type.Is(other.Type)
               && Tags.Is(other.Tags);
    }

    public override SearchIndexEntry Clone()
    {
        return new SearchIndexEntry
        {
            Title = Title,
            Description = Description,
            Content = Content,
            Url = Url,
            Type = Type,
            Tags = Tags.ToList()
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    #endregion

}