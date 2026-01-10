using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.ViewModels.Blog;

public class BlogListViewModel : ViewModelBase
{
    #region Functions

    protected string GetPageUrl(int page)
    {
        return page == 1 ? BaseUrl : $"{BaseUrl}/page/{page}";
    }
    

    #endregion
    #region Parameters

    [Parameter]
    public List<BlogPost> Posts { get; set; } = [];

    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int TotalPages { get; set; } = 1;

    [Parameter]
    public string BaseUrl { get; set; } = "/blog";

    #endregion

}