using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Sidebar item for documentation navigation.
/// </summary>
public class DocsSidebarItem : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not DocsSidebarItem other)
            return false;

        return Title.Is(other.Title)
            && Href.Is(other.Href);
    }

    public override DocsSidebarItem Clone()
    {
        return new DocsSidebarItem
        {
            Title = Title,
            Href = Href
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string Title { get; set; } = string.Empty;

    public string Href { get; set; } = string.Empty;

    #endregion

}