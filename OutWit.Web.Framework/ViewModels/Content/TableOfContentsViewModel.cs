using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.ViewModels.Content;

public class TableOfContentsViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    public List<TocEntry> Entries { get; set; } = [];

    #endregion
}
