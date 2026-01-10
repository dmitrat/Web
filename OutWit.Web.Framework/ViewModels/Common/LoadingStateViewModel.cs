using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class LoadingStateViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public bool IsLoading { get; set; }
    
    [Parameter]
    public bool HasContent { get; set; }
    
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";
    
    [Parameter]
    public string NotFoundText { get; set; } = "Content not found.";
    
    [Parameter]
    public string? BackLinkUrl { get; set; }
    
    [Parameter]
    public string BackLinkText { get; set; } = "← Back";
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}