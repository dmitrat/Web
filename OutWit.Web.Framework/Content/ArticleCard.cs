using OutWit.Common.Abstract;
using OutWit.Common.Collections;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Model for article/doc pages with table of contents sidebar.
/// </summary>
public class ArticleCard : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not ArticleCard other)
            return false;
        
        return Slug.Is(other.Slug)
            && Order.Is(other.Order)
            && Title.Is(other.Title)
            && Description.Is(other.Description)
            && PublishDate.Is(other.PublishDate)
            && Tags.Is(other.Tags)
            && MenuTitle.Is(other.MenuTitle)
            && ShowInMenu.Is(other.ShowInMenu)
            && RawContent.Is(other.RawContent)
            && HtmlContent.Is(other.HtmlContent)
            && TableOfContents.Is(other.TableOfContents)
            && EmbeddedComponents.Is(other.EmbeddedComponents);
    }

    public override ArticleCard Clone()
    {
        return new ArticleCard
        {
            Slug = Slug,
            Order = Order,
            Title = Title,
            Description = Description,
            PublishDate = PublishDate,
            Tags = Tags,
            MenuTitle = MenuTitle,
            ShowInMenu = ShowInMenu,
            RawContent = RawContent,
            HtmlContent = HtmlContent,
            TableOfContents = TableOfContents.Select(toc => toc.Clone()).ToList(),
            EmbeddedComponents = EmbeddedComponents.Select(component => component.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    public string Slug { get; set; } = "";
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime PublishDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string MenuTitle { get; set; } = "";
    public bool ShowInMenu { get; set; } = true;
    public string RawContent { get; set; } = "";
    public string HtmlContent { get; set; } = "";
    public List<TocItem> TableOfContents { get; set; } = new();
    public List<EmbeddedComponent> EmbeddedComponents { get; set; } = new();

    #endregion

}