using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class ContactPageViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public string Title { get; set; } = "Contact";
    
    [Parameter]
    public string? IntroText { get; set; }
    
    [Parameter]
    public string SeoTitle { get; set; } = "Contact";
    
    [Parameter]
    public string SeoDescription { get; set; } = string.Empty;

    #endregion
}
