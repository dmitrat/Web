using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.ViewModels.Blog;

public class BlogCardViewModel : ViewModelBase
{
    #region Parameters

    [Parameter]
    [EditorRequired]
    public BlogPost Post { get; set; } = null!;

    #endregion
}