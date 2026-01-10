using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.ViewModels.Docs;

public class DocsNavFooterViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public DocNavLink? Previous { get; set; }

    [Parameter]
    public DocNavLink? Next { get; set; }

    #endregion
}
