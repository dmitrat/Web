using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Navigation menu item.
/// </summary>
public class NavItem : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not NavItem other)
            return false;

        return Title.Is(other.Title)
                && Href.Is(other.Href)
                && External.Is(other.External)
                && Children.Is(other.Children);
    }

    public override NavItem Clone()
    {
        return new NavItem
        {
            Title = Title,
            Href = Href,
            External = External,
            Children = Children.Select(item => item.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string Title { get; set; } = string.Empty;

    public string Href { get; set; } = string.Empty;

    public bool External { get; set; }

    public List<NavItem> Children { get; set; } = [];

    #endregion

}