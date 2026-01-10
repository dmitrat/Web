using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Navigation link for prev/next doc pages.
/// </summary>
public class DocNavLink: ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not DocNavLink other)
            return false;

        return Slug.Is(other.Slug)
            && Title.Is(other.Title);
    }

    public override DocNavLink Clone()
    {
        return new DocNavLink
        {
            Slug = Slug,
            Title = Title
        };
    }

    #endregion

    #region Properties

    public string Slug { get; set; } = string.Empty;

    [ToString]
    public string Title { get; set; } = string.Empty;

    #endregion

}