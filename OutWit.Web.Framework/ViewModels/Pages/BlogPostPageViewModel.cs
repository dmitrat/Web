using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class BlogPostPageViewModel : ViewModelBase
{
    #region Fields

    private BlogPost? m_post;
    private bool m_loading = true;

    #endregion

    #region Initialization

    protected override async Task OnParametersSetAsync()
    {
        m_loading = true;
        m_post = await ContentService.GetBlogPostAsync(Slug);
        m_loading = false;
    }

    #endregion

    #region Properties

    protected BlogPost? Post => m_post;
    
    protected bool Loading => m_loading;
    
    protected string PageTitle => m_post?.Title != null 
        ? $"{m_post.Title} - {SiteName}" 
        : $"Blog Post - {SiteName}";

    #endregion

    #region Parameters

    [Parameter, EditorRequired]
    public string Slug { get; set; } = string.Empty;
    
    [Parameter]
    public string SiteName { get; set; } = "OutWit";
    
    [Parameter]
    public string BackUrl { get; set; } = "/blog";
    
    [Parameter]
    public string BackLinkText { get; set; } = "\u2190 Back to Blog";

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;

    #endregion
}
