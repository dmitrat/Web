using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class ProjectPageViewModel : ViewModelBase
{
    #region Fields

    private ProjectCard? m_project;
    private bool m_loading = true;
    private string m_processedContent = string.Empty;
    private List<EmbeddedComponent> m_embeddedComponents = [];
    private string m_loadedSlug = string.Empty;

    #endregion

    #region Initialization

    protected override async Task OnInitializedAsync()
    {
        await LoadProject();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Slug != m_loadedSlug)
        {
            m_loading = true;
            StateHasChanged();
            await LoadProject();
        }
    }

    #endregion

    #region Functions

    private async Task LoadProject()
    {
        if (string.IsNullOrEmpty(Slug)) 
        {
            m_loading = false;
            return;
        }
        
        m_loadedSlug = Slug;
        
        try 
        {
            m_project = await ContentService.GetProjectAsync(Slug);
            
            if (m_project != null)
            {
                m_embeddedComponents = m_project.EmbeddedComponents;
                m_processedContent = m_project.HtmlContent;
            }
            else
            {
                m_embeddedComponents = [];
                m_processedContent = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading project: {ex.Message}");
            m_project = null;
            m_embeddedComponents = [];
            m_processedContent = string.Empty;
        }
        m_loading = false;
    }

    #endregion

    #region Properties

    protected ProjectCard? Project => m_project;
    
    protected bool Loading => m_loading;
    
    protected string ProcessedContent => m_processedContent;
    
    protected List<EmbeddedComponent> EmbeddedComponents => m_embeddedComponents;

    #endregion

    #region Parameters

    [Parameter, EditorRequired]
    public string Slug { get; set; } = string.Empty;
    
    [Parameter]
    public string BackUrl { get; set; } = "/";
    
    [Parameter]
    public string BackLinkText { get; set; } = "\u2190 Back to Home";
    
    [Parameter]
    public string VisitButtonText { get; set; } = "Visit Project \u2192";

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;
    
    [Inject]
    public ComponentRegistry ComponentRegistry { get; set; } = null!;

    #endregion
}
