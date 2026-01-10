using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Represents a blog post with frontmatter and content.
/// </summary>
public class BlogPost : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not BlogPost other)
            return false;

        return Slug.Is(other.Slug)
            && Title.Is(other.Title)
            && Description.Is(other.Description)
            && Summary.Is(other.Summary)
            && PublishDate.Is(other.PublishDate)
            && ModifiedDate.Is(other.ModifiedDate)
            && Author.Is(other.Author)
            && Tags.Is(other.Tags)
            && ReadingTimeMinutes.Is(other.ReadingTimeMinutes)
            && FeaturedImage.Is(other.FeaturedImage)
            && RawContent.Is(other.RawContent)
            && HtmlContent.Is(other.HtmlContent)
            && IsDraft.Is(other.IsDraft);
    }

    public override BlogPost Clone()
    {
        return new BlogPost
        {
            Slug = Slug,
            Title = Title,
            Description = Description,
            Summary = Summary,
            PublishDate = PublishDate,
            ModifiedDate = ModifiedDate,
            Author = Author,
            Tags = Tags.ToList(),
            ReadingTimeMinutes = ReadingTimeMinutes,
            FeaturedImage = FeaturedImage,
            RawContent = RawContent,
            HtmlContent = HtmlContent,
            IsDraft = IsDraft

        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// URL slug for the blog post.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Post title.
    /// </summary>
    [ToString]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short description or excerpt.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Post summary for previews.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Publication date.
    /// </summary>
    public DateTime PublishDate { get; set; }

    /// <summary>
    /// Last modification date.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Author name.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Tags/categories for the post.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Estimated reading time in minutes.
    /// </summary>
    public int ReadingTimeMinutes { get; set; }

    /// <summary>
    /// Featured image URL.
    /// </summary>
    public string? FeaturedImage { get; set; }

    /// <summary>
    /// Raw markdown content.
    /// </summary>
    public string RawContent { get; set; } = string.Empty;

    /// <summary>
    /// Rendered HTML content.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the post is a draft.
    /// </summary>
    public bool IsDraft { get; set; }

    #endregion
}
