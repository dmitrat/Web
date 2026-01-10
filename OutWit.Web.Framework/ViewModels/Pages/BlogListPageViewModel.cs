using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class BlogListPageViewModel : ViewModelBase
{
    #region Fields

    private List<BlogPost> m_posts = [];
    private bool m_loading = true;

    #endregion

    #region Initialization

    protected override async Task OnInitializedAsync()
    {
        m_posts = await ContentService.GetBlogPostsAsync();
        m_loading = false;
    }

    #endregion

    #region Functions

    protected string GetPostUrl(string slug) => $"{PostUrlPrefix}/{slug}";

    #endregion

    #region Properties

    protected List<BlogPost> Posts => m_posts;
    
    protected bool Loading => m_loading;

    #endregion

    #region Parameters

    [Parameter]
    public string Title { get; set; } = string.Empty;
    
    [Parameter]
    public string? Description { get; set; }
    
    [Parameter]
    public string SeoTitle { get; set; } = "Blog";
    
    [Parameter]
    public string SeoDescription { get; set; } = string.Empty;
    
    [Parameter]
    public string SidebarTitle { get; set; } = "Recent posts";
    
    [Parameter]
    public int SidebarMaxPosts { get; set; } = 10;
    
    [Parameter]
    public string PostUrlPrefix { get; set; } = "/blog";

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;

    #endregion
}
