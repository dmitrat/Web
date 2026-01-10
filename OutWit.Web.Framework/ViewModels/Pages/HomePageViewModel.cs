using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class HomePageViewModel : ViewModelBase
{
    #region Fields

    private List<ProjectCard> m_projects = [];
    private bool m_loading = true;

    #endregion

    #region Initialization

    protected override async Task OnInitializedAsync()
    {
        try 
        {
            m_projects = await ContentService.GetProjectsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading projects: {ex.Message}");
            m_projects = [];
        }
        m_loading = false;
    }

    #endregion

    #region Functions

    protected MarkupString RenderSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
            return new MarkupString(string.Empty);
        
        return new MarkupString(MarkdownService.ToHtml(summary));
    }
    
    protected string GetFirstProjectAnchor()
    {
        var firstProject = m_projects.FirstOrDefault(p => p.IsFirstProject);
        if (firstProject != null)
            return $"#project-{firstProject.Slug}";
        
        firstProject = m_projects.FirstOrDefault(p => !p.ShowInHeader);
        if (firstProject != null)
            return $"#project-{firstProject.Slug}";
        
        return "#projects";
    }

    #endregion

    #region Properties

    protected List<ProjectCard> Projects => m_projects;
    
    protected bool Loading => m_loading;

    #endregion

    #region Parameters

    [Parameter]
    public RenderFragment? HeroContent { get; set; }
    
    [Parameter]
    public string? ProjectsSectionTitle { get; set; }
    
    [Parameter]
    public string SeoTitle { get; set; } = "Home";
    
    [Parameter]
    public string SeoDescription { get; set; } = string.Empty;

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; set; } = null!;
    
    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    #endregion
}
