using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Content;

public class AdmonitionViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public string Type { get; set; } = "note";

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
