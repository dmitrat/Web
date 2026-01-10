using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Model for project cards on the home page.
/// </summary>
public class ProjectCard : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not ProjectCard other)
            return false;
        
        return Slug.Is(other.Slug)
            && Order.Is(other.Order)
            && Title.Is(other.Title)
            && Description.Is(other.Description)
            && Summary.Is(other.Summary)
            && Tags.Is(other.Tags)
            && Url.Is(other.Url)
            && MenuTitle.Is(other.MenuTitle)
            && ShowInMenu.Is(other.ShowInMenu)
            && ShowInHeader.Is(other.ShowInHeader)
            && IsFirstProject.Is(other.IsFirstProject)
            && RawContent.Is(other.RawContent)
            && HtmlContent.Is(other.HtmlContent)
            && EmbeddedComponents.Is(other.EmbeddedComponents);
    }

    public override ProjectCard Clone()
    {
        return new ProjectCard
        {
            Slug = Slug,
            Order = Order,
            Title = Title,
            Description = Description,
            Summary = Summary,
            Tags = Tags,
            Url = Url,
            MenuTitle = MenuTitle,
            ShowInMenu = ShowInMenu,
            ShowInHeader = ShowInHeader,
            IsFirstProject = IsFirstProject,
            RawContent = RawContent,
            HtmlContent = HtmlContent,
            EmbeddedComponents = EmbeddedComponents.Select(component => component.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    public string Slug { get; set; } = "";
    public int Order { get; set; }
    
    [ToString]
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string Url { get; set; } = "";
    public string MenuTitle { get; set; } = "";
    public bool ShowInMenu { get; set; } = true;
    public bool ShowInHeader { get; set; } = false;
    public bool IsFirstProject { get; set; } = false;
    public string RawContent { get; set; } = "";
    public string HtmlContent { get; set; } = "";
    public List<EmbeddedComponent> EmbeddedComponents { get; set; } = new();

    #endregion
}