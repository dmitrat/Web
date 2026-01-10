using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class NotFoundPageViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public string Title { get; set; } = "Page Not Found";
    
    [Parameter]
    public string Message { get; set; } = "Sorry, the page you're looking for doesn't exist.";
    
    [Parameter]
    public string HomeUrl { get; set; } = "/";
    
    [Parameter]
    public string HomeButtonText { get; set; } = "Go Home";

    #endregion
}
