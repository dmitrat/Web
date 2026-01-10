using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class SocialIconViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public string Platform { get; set; } = string.Empty;

    [Parameter]
    public int Size { get; set; } = 24;

    #endregion
}