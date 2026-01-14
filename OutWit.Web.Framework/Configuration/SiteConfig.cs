using OutWit.Web.Framework.Models;
using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Configuration;

/// <summary>
/// Site configuration loaded from site.config.json.
/// </summary>
public class SiteConfig : ModelBase
{

    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not SiteConfig other)
            return false;
        
        return SiteName.Is(other.SiteName)
            && BaseUrl.Is(other.BaseUrl)
            && LogoLight.Is(other.LogoLight)
            && LogoDark.Is(other.LogoDark)
            && DefaultTheme.Is(other.DefaultTheme)
            && Navigation.Is(other.Navigation)
            && Footer.Is(other.Footer)
            && Contact.Is(other.Contact)
            && Search.Is(other.Search)
            && Seo.Is(other.Seo)
            && ContentSections.Is(other.ContentSections);
    }

    public override SiteConfig Clone()
    {
        return new SiteConfig
        {
            SiteName = SiteName,
            BaseUrl = BaseUrl,
            LogoLight = LogoLight,
            LogoDark = LogoDark,
            DefaultTheme = DefaultTheme,
            Navigation = Navigation.Select(item => item.Clone()).ToList(),
            Footer = Footer.Clone(),
            Contact = Contact.Clone(),
            Search = Search.Clone(),
            Seo = Seo.Clone(),
            ContentSections = ContentSections.Select(s => s.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Site name displayed in header and title.
    /// </summary>
    [ToString]
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the site (used for canonical URLs and sitemap).
    /// </summary>
    [ToString]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Path to site logo for light theme (dark logo on light background).
    /// </summary>
    public string LogoLight { get; set; } = "/images/logo-light.svg";

    /// <summary>
    /// Path to site logo for dark theme (light logo on dark background).
    /// </summary>
    public string LogoDark { get; set; } = "/images/logo-dark.svg";

    /// <summary>
    /// Default theme (dark/light).
    /// </summary>
    public string DefaultTheme { get; set; } = "dark";

    /// <summary>
    /// Navigation menu items.
    /// </summary>
    public List<NavItem> Navigation { get; set; } = [];

    /// <summary>
    /// Footer configuration.
    /// </summary>
    public FooterConfig Footer { get; set; } = new();

    /// <summary>
    /// Contact form configuration.
    /// </summary>
    public ContactConfig Contact { get; set; } = new();

    /// <summary>
    /// Search configuration.
    /// </summary>
    public SearchConfig Search { get; set; } = new();

    /// <summary>
    /// SEO configuration.
    /// </summary>
    public SeoConfig Seo { get; set; } = new();

    /// <summary>
    /// Custom content sections (like docs/articles but with custom names).
    /// Allows creating sections like "solutions", "products", etc.
    /// </summary>
    public List<ContentSectionConfig> ContentSections { get; set; } = [];

    #endregion

  
}