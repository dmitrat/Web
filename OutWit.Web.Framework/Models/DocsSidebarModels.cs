using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Sidebar section for documentation navigation.
/// </summary>
public class DocsSidebarSection : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not DocsSidebarSection other)
            return false;

        return Title.Is(other.Title)
               && Items.Is(other.Items);
    }

    public override DocsSidebarSection Clone()
    {
        return new DocsSidebarSection
        {
            Title = Title,
            Items = Items.Select(item => item.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string Title { get; set; } = string.Empty;

    public List<DocsSidebarItem> Items { get; set; } = [];

    #endregion

}