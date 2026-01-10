using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class HeroRectViewModel : ViewModelBase
{
    #region Parameters

    /// <summary>
    /// Logo URL for dark theme (light logo on dark background).
    /// If only LogoUrl is set, it will be used for both themes.
    /// </summary>
    [Parameter]
    public string LogoDarkUrl { get; set; } = "";
    
    /// <summary>
    /// Logo URL for light theme (dark logo on light background).
    /// If only LogoUrl is set, it will be used for both themes.
    /// </summary>
    [Parameter]
    public string LogoLightUrl { get; set; } = "";
    
    /// <summary>
    /// Single logo URL (used for both themes if LogoDarkUrl/LogoLightUrl not set).
    /// Deprecated: prefer LogoDarkUrl and LogoLightUrl for theme support.
    /// </summary>
    [Parameter]
    public string LogoUrl { get; set; } = "";
    
    [Parameter]
    public string Title { get; set; } = "";
    
    [Parameter]
    public string Tagline { get; set; } = "";
    
    [Parameter]
    public string BadgeText { get; set; } = "";
    
    [Parameter]
    public bool ShowActions { get; set; } = true;
    
    [Parameter]
    public string PrimaryButtonText { get; set; } = "Get Started";
    
    [Parameter]
    public string PrimaryButtonUrl { get; set; } = "/docs";
    
    [Parameter]
    public EventCallback OnPrimaryButtonClick { get; set; }
    
    [Parameter]
    public string SecondaryButtonText { get; set; } = "GitHub";
    
    [Parameter]
    public string SecondaryButtonUrl { get; set; } = "";
    
    [Parameter]
    public bool SecondaryButtonExternal { get; set; } = true;
    
    [Parameter]
    public EventCallback OnSecondaryButtonClick { get; set; }
    
    [Parameter]
    public RenderFragment? SecondaryButtonIcon { get; set; }

    #endregion

    #region Properties

    /// <summary>
    /// Resolved dark theme logo URL.
    /// </summary>
    protected string ResolvedLogoDarkUrl => !string.IsNullOrEmpty(LogoDarkUrl) ? LogoDarkUrl : LogoUrl;
    
    /// <summary>
    /// Resolved light theme logo URL.
    /// </summary>
    protected string ResolvedLogoLightUrl => !string.IsNullOrEmpty(LogoLightUrl) ? LogoLightUrl : LogoUrl;
    
    /// <summary>
    /// Whether we have separate logos for themes.
    /// </summary>
    protected bool HasThemeLogos => !string.IsNullOrEmpty(LogoDarkUrl) || !string.IsNullOrEmpty(LogoLightUrl);
    
    /// <summary>
    /// Whether any logo is set.
    /// </summary>
    protected bool HasLogo => !string.IsNullOrEmpty(LogoUrl) || !string.IsNullOrEmpty(LogoDarkUrl) || !string.IsNullOrEmpty(LogoLightUrl);

    #endregion
}