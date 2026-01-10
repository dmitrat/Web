using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Base metadata for all content pages.
/// </summary>
public class PageMetadata : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not PageMetadata other)
            return false;

        return Title.Is(other.Title)
            && Description.Is(other.Description)
            && CanonicalUrl.Is(other.CanonicalUrl)
            && SocialImage.Is(other.SocialImage)
            && Keywords.Is(other.Keywords)
            && NoIndex.Is(other.NoIndex)
            && OgType.Is(other.OgType);
    }

    public override PageMetadata Clone()
    {
        return new PageMetadata
        {
            Title = Title,
            Description = Description,
            CanonicalUrl = CanonicalUrl,
            SocialImage = SocialImage,
            Keywords = Keywords.ToList(),
            NoIndex = NoIndex,
            OgType = OgType
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Page title for display and SEO.
    /// </summary>
    [ToString]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Page description for SEO meta tags.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Canonical URL for SEO.
    /// </summary>
    public string? CanonicalUrl { get; set; }

    /// <summary>
    /// Social media preview image URL.
    /// </summary>
    public string? SocialImage { get; set; }

    /// <summary>
    /// Page keywords for SEO.
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// Indicates if the page should be indexed by search engines.
    /// </summary>
    public bool NoIndex { get; set; }

    /// <summary>
    /// Open Graph type (website, article, etc.).
    /// </summary>
    public string OgType { get; set; } = "website";

    #endregion
}
