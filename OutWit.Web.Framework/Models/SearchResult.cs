using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Search result item.
/// </summary>
public class SearchResult : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not SearchResult other)
            return false;

        return Title.Is(other.Title)
            && Description.Is(other.Description)
            && Url.Is(other.Url)
            && ContentType.Is(other.ContentType)
            && Score.Is(other.Score, tolerance)
            && MatchedTerms.Is(other.MatchedTerms);
    }

    public override SearchResult Clone()
    {
        return new SearchResult
        {
            Title = Title,
            Description = Description,
            Url = Url,
            ContentType = ContentType,
            Score = Score,
            MatchedTerms = MatchedTerms.ToList()
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Title of the matching item.
    /// </summary>
    [ToString]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description or excerpt with match context.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL to the matching page.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Type of content (blog, doc, page).
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score (0-1).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Matched terms for highlighting.
    /// </summary>
    public List<string> MatchedTerms { get; set; } = [];

    #endregion

}