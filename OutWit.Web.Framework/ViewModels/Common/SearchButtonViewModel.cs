using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class SearchButtonViewModel : ViewModelBase
{
    #region Functions

    protected async Task OpenSearch()
    {
        await OnClick.InvokeAsync();
    }

    #endregion

    #region Parameters

    [Parameter]
    public EventCallback OnClick { get; set; }

    #endregion

}