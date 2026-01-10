using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Model for feature cards on product pages.
/// </summary>
public class FeatureCard : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not FeatureCard other)
            return false;
        
        return Slug.Is(other.Slug)
            && Order.Is(other.Order)
            && Title.Is(other.Title)
            && Description.Is(other.Description)
            && Icon.Is(other.Icon)
            && HtmlContent.Is(other.HtmlContent);
    }

    public override FeatureCard Clone()
    {
        return new FeatureCard
        {
            Slug = Slug,
            Order = Order,
            Title = Title,
            Description = Description,
            Icon = Icon,
            HtmlContent = HtmlContent
        };
    }

    #endregion

    #region Properties

    public string Slug { get; set; } = "";
    public int Order { get; set; }
    
    [ToString]
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";  // Emoji or path to SVG
    public string HtmlContent { get; set; } = "";

    #endregion

}