using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class PageHeaderViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = "";
    
    [Parameter]
    public DateTime? Date { get; set; }
    
    [Parameter]
    public int ReadingTimeMinutes { get; set; }
    
    [Parameter]
    public bool ShowMeta { get; set; } = true;
    
    [Parameter]
    public List<string>? Tags { get; set; }
    
    [Parameter]
    public string? BackLinkUrl { get; set; }
    
    [Parameter]
    public string BackLinkText { get; set; } = "\u2190 Back";
    
    [Parameter]
    public string? CssClass { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}