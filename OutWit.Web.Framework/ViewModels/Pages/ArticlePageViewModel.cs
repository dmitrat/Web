using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class ArticlePageViewModel : ViewModelBase
{
    #region Fields

    private ArticleCard? m_article;
    private bool m_loading = true;
    private string m_processedContent = string.Empty;
    private List<EmbeddedComponent> m_embeddedComponents = [];
    private string m_activeHeadingId = string.Empty;

    #endregion

    #region Initialization

    protected override async Task OnParametersSetAsync()
    {
        await LoadArticle();
    }

    #endregion

    #region Functions

    private async Task LoadArticle()
    {
        m_loading = true;
        StateHasChanged();

        try
        {
            m_article = await ContentService.GetArticleAsync(Slug, ContentFolder);
            
            if (m_article != null)
            {
                m_processedContent = m_article.HtmlContent;
                m_embeddedComponents = m_article.EmbeddedComponents;
                
                if (m_article.TableOfContents.Count > 0)
                {
                    m_activeHeadingId = m_article.TableOfContents[0].Id;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading article: {ex.Message}");
            m_article = null;
        }

        m_loading = false;
    }
    
    protected async Task ScrollToHeading(string headingId)
    {
        m_activeHeadingId = headingId;
        await JS.InvokeVoidAsync("scrollToElement", headingId);
    }

    #endregion

    #region Properties

    protected ArticleCard? Article => m_article;
    
    protected bool Loading => m_loading;
    
    protected string ProcessedContent => m_processedContent;
    
    protected List<EmbeddedComponent> EmbeddedComponents => m_embeddedComponents;
    
    protected string ActiveHeadingId => m_activeHeadingId;

    #endregion

    #region Parameters

    [Parameter, EditorRequired]
    public string Slug { get; set; } = string.Empty;
    
    [Parameter]
    public bool ShowToc { get; set; } = true;
    
    [Parameter]
    public string TocTitle { get; set; } = "ON THIS PAGE";
    
    [Parameter]
    public string BackUrl { get; set; } = "/";
    
    [Parameter]
    public string BackLinkText { get; set; } = "\u2190 Back";
    
    [Parameter]
    public string ContentFolder { get; set; } = "articles";

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;
    
    [Inject]
    public ComponentRegistry ComponentRegistry { get; set; } = null!;
    
    [Inject]
    public IJSRuntime JS { get; set; } = null!;

    #endregion
}
