using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class DocPageViewModel : ViewModelBase
{
    #region Fields

    private Models.DocPage? m_doc;
    private bool m_loading = true;
    private string m_activeHeadingId = string.Empty;
    private string m_lastLoadedSlug = string.Empty;

    #endregion

    #region Initialization

    protected override async Task OnParametersSetAsync()
    {
        if (Slug != m_lastLoadedSlug)
        {
            await LoadDoc();
        }
    }

    #endregion

    #region Functions

    private async Task LoadDoc()
    {
        m_loading = true;
        m_lastLoadedSlug = Slug;
        StateHasChanged();

        try
        {
            m_doc = await ContentService.GetDocAsync(Slug);
            
            if (m_doc != null && m_doc.TableOfContents.Count > 0)
            {
                m_activeHeadingId = m_doc.TableOfContents[0].Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading doc: {ex.Message}");
            m_doc = null;
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

    protected Models.DocPage? Doc => m_doc;
    
    protected bool Loading => m_loading;
    
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

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;
    
    [Inject]
    public IJSRuntime JS { get; set; } = null!;

    #endregion
}
