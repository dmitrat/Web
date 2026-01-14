using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class FeatureCardViewModel : ViewModelBase
{
    #region Parameters

    /// <summary>
    /// Emoji or text icon (simple option)
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "";
    
    /// <summary>
    /// Custom icon content (SVG, etc.) as RenderFragment
    /// </summary>
    [Parameter]
    public RenderFragment? IconContent { get; set; }
    
    /// <summary>
    /// Inline SVG content as string (from markdown iconSvg field)
    /// </summary>
    [Parameter]
    public string IconSvg { get; set; } = "";
    
    [Parameter]
    public string Title { get; set; } = "";
    
    /// <summary>
    /// Simple description text (supports HTML)
    /// </summary>
    [Parameter]
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Custom description content for complex markup
    /// </summary>
    [Parameter]
    public RenderFragment? DescriptionContent { get; set; }

    #endregion
}