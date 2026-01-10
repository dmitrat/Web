using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Generic frontmatter data class matching YAML structure in markdown files.
/// </summary>
public class FrontmatterData : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not FrontmatterData other)
            return false;
        
        return Title.Is(other.Title)
            && Description.Is(other.Description)
            && Summary.Is(other.Summary)
            && PublishDate.Is(other.PublishDate)
            && Tags.Is(other.Tags)
            && FeaturedImage.Is(other.FeaturedImage)
            && Author.Is(other.Author)
            && Url.Is(other.Url)
            && MenuTitle.Is(other.MenuTitle)
            && ShowInMenu.Is(other.ShowInMenu)
            && ShowInHeader.Is(other.ShowInHeader)
            && IsFirstProject.Is(other.IsFirstProject)
            && Parent.Is(other.Parent)
            && Icon.Is(other.Icon);
    }

    public override FrontmatterData Clone()
    {
        return new FrontmatterData
        {
            Title = Title,
            Description = Description,
            Summary = Summary,
            PublishDate = PublishDate,
            Tags = Tags?.ToList(),
            FeaturedImage = FeaturedImage,
            Author = Author,
            Url = Url,
            MenuTitle = MenuTitle,
            ShowInMenu = ShowInMenu,
            ShowInHeader = ShowInHeader,
            IsFirstProject = IsFirstProject,
            Parent = Parent,
            Icon = Icon
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Summary { get; set; }
    public DateTime PublishDate { get; set; }
    public List<string>? Tags { get; set; }
    public string? FeaturedImage { get; set; }
    public string? Author { get; set; }
    public string? Url { get; set; }
    public string? MenuTitle { get; set; }
    public bool ShowInMenu { get; set; } = true;
    public bool ShowInHeader { get; set; } = false;
    public bool IsFirstProject { get; set; } = false;
    public string? Parent { get; set; }
    public string? Icon { get; set; }  // Emoji or path to SVG

    #endregion
 
}