using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Footer link group.
/// </summary>
public class FooterLinkGroup : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not FooterLinkGroup other)
            return false;

        return Title.Is(other.Title)
                && Links.Is(other.Links);
    }

    public override FooterLinkGroup Clone()
    {
        return new FooterLinkGroup
        {
            Title = Title,
            Links = Links.Select(item => item.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string Title { get; set; } = string.Empty;

    public List<NavItem> Links { get; set; } = [];

    #endregion

}