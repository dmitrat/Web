using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Represents a documentation page with navigation metadata.
/// </summary>
public class DocPage : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not DocPage other)
            return false;

        return Slug.Is(other.Slug)
            && Title.Is(other.Title)
            && Description.Is(other.Description)
            && Keywords.Is(other.Keywords)
            && Order.Is(other.Order)
            && ParentSlug.Is(other.ParentSlug)
            && RawContent.Is(other.RawContent)
            && HtmlContent.Is(other.HtmlContent)
            && TableOfContents.Is(other.TableOfContents)
            && PreviousPage.Check(other.PreviousPage)
            && NextPage.Check(other.NextPage);
    }

    public override DocPage Clone()
    {
        return new DocPage
        {
            Slug = Slug,
            Title = Title,
            Description = Description,
            Keywords = Keywords.ToList(),
            Order = Order,
            ParentSlug = ParentSlug,
            RawContent = RawContent,
            HtmlContent = HtmlContent,
            TableOfContents = TableOfContents.Select(toc => toc.Clone()).ToList(),
            PreviousPage = PreviousPage?.Clone(),
            NextPage = NextPage?.Clone()
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// URL slug for the doc page.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Page title.
    /// </summary>
    [ToString]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Keywords for search and SEO.
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// Sort order in sidebar navigation.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Parent section/category slug (for hierarchy).
    /// </summary>
    public string? ParentSlug { get; set; }

    /// <summary>
    /// Raw markdown content.
    /// </summary>
    public string RawContent { get; set; } = string.Empty;

    /// <summary>
    /// Rendered HTML content.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Table of contents extracted from headings.
    /// </summary>
    public List<TocEntry> TableOfContents { get; set; } = [];

    /// <summary>
    /// Previous page in navigation order.
    /// </summary>
    public DocNavLink? PreviousPage { get; set; }

    /// <summary>
    /// Next page in navigation order.
    /// </summary>
    public DocNavLink? NextPage { get; set; }

    #endregion

}