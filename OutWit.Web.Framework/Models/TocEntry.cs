using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Table of contents entry.
/// </summary>
public class TocEntry : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not TocEntry other)
            return false;

        return Id.Is(other.Id)
            && Title.Is(other.Title)
            && Level.Is(other.Level)
            && Children.Is(other.Children);
    }
    public override TocEntry Clone()
    {
        return new TocEntry
        {
            Id = Id,
            Title = Title,
            Level = Level,
            Children = Children.Select(entry => entry.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    public string Id { get; set; } = string.Empty;

    [ToString]
    public string Title { get; set; } = string.Empty;

    public int Level { get; set; }

    public List<TocEntry> Children { get; set; } = [];

    #endregion

}