using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Configuration;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Layout;

public class HeaderViewModel : ViewModelBase, IDisposable
{
    #region Fields

    private SiteConfig? m_config;
    private bool m_mobileMenuOpen;
    private string m_searchQuery = string.Empty;

    #endregion

    #region Initialization

    protected override async Task OnInitializedAsync()
    {
        m_config = await ConfigService.GetConfigAsync();
        
        await PopulateDynamicMenus();
        
        NavigationManager.LocationChanged += OnLocationChanged;
        ThemeService.ThemeChanged += OnThemeChanged;
    }

    private async Task PopulateDynamicMenus()
    {
        if (m_config?.Navigation == null)
            return;
            
        var articles = await ContentService.GetArticlesAsync();
        var projects = await ContentService.GetProjectsAsync();
        var docs = await ContentService.GetDocsAsync();
        
        foreach (var navItem in m_config.Navigation)
        {
            PopulateArticlesDropdown(navItem, articles);
            PopulateDocsDropdown(navItem, docs);
            PopulateProjectsDropdown(navItem, projects);
        }
        
        AddHeaderProjects(projects);
    }

    private void PopulateArticlesDropdown(NavItem navItem, List<ArticleCard> articles)
    {
        if (navItem.Href != "/articles" || navItem.Children.Count != 0)
            return;
            
        navItem.Children = articles
            .Where(a => a.ShowInMenu)
            .OrderBy(a => a.Order)
            .Select(a => new NavItem
            {
                Title = !string.IsNullOrEmpty(a.MenuTitle) ? a.MenuTitle : a.Title,
                Href = $"/article/{a.Slug}"
            })
            .ToList();
    }

    private void PopulateDocsDropdown(NavItem navItem, List<DocPage> docs)
    {
        if (navItem.Href != "/docs" || navItem.Children.Count != 0)
            return;
            
        navItem.Children = docs
            .OrderBy(d => d.Order)
            .Select(d => new NavItem
            {
                Title = d.Title,
                Href = $"/docs/{d.Slug}"
            })
            .ToList();
    }

    private void PopulateProjectsDropdown(NavItem navItem, List<ProjectCard> projects)
    {
        if ((navItem.Href != "/#projects" && navItem.Href != "/projects") || navItem.Children.Count != 0)
            return;
            
        navItem.Children = projects
            .Where(p => p.ShowInMenu)
            .OrderBy(p => p.Order)
            .Select(p => new NavItem
            {
                Title = !string.IsNullOrEmpty(p.MenuTitle) ? p.MenuTitle : p.Title,
                Href = $"/project/{p.Slug}"
            })
            .ToList();
    }

    private void AddHeaderProjects(List<ProjectCard> projects)
    {
        if (m_config?.Navigation == null)
            return;
            
        var headerProjects = projects
            .Where(p => p.ShowInHeader)
            .OrderBy(p => p.Order)
            .Select(p => new NavItem
            {
                Title = !string.IsNullOrEmpty(p.MenuTitle) ? p.MenuTitle : p.Title,
                Href = $"/project/{p.Slug}"
            })
            .ToList();
        
        var contactIndex = m_config.Navigation.FindIndex(n => 
            n.Href.Equals("/contact", StringComparison.OrdinalIgnoreCase));
        var insertIndex = contactIndex >= 0 ? contactIndex : m_config.Navigation.Count;
        
        foreach (var headerProject in headerProjects)
        {
            if (!m_config.Navigation.Any(n => n.Href == headerProject.Href))
            {
                m_config.Navigation.Insert(insertIndex, headerProject);
                insertIndex++;
            }
        }
    }

    #endregion

    #region Functions

    protected string GetCurrentPath()
    {
        return "/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
    }

    protected bool IsActive(string href)
    {
        var currentPath = GetCurrentPath();
        if (href == "/" && (currentPath == "/" || currentPath == ""))
            return true;
        if (href != "/" && currentPath.StartsWith(href))
            return true;
        return false;
    }
    
    protected bool IsActiveDropdown(NavItem navItem)
    {
        if (navItem.Children?.Count > 0)
            return navItem.Children.Any(child => IsActive(child.Href));
        return IsActive(navItem.Href);
    }

    protected void ToggleMobileMenu()
    {
        m_mobileMenuOpen = !m_mobileMenuOpen;
    }

    protected void CloseMobileMenu()
    {
        m_mobileMenuOpen = false;
    }
    
    protected void HandleSearch(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(m_searchQuery))
        {
            NavigationManager.NavigateTo($"/search?q={Uri.EscapeDataString(m_searchQuery)}");
        }
    }
    
    protected void TriggerSearch()
    {
        if (!string.IsNullOrWhiteSpace(m_searchQuery))
        {
            NavigationManager.NavigateTo($"/search?q={Uri.EscapeDataString(m_searchQuery)}");
        }
    }
    
    protected async Task ToggleTheme()
    {
        await ThemeService.ToggleThemeAsync();
    }

    #endregion

    #region Event Handlers
    
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnThemeChanged(ThemeMode theme)
    {
        StateHasChanged();
    }

    #endregion

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        ThemeService.ThemeChanged -= OnThemeChanged;
        base.Dispose(disposing);
    }

    #endregion

    #region Properties

    protected SiteConfig? Config => m_config;
    
    protected bool MobileMenuOpen
    {
        get => m_mobileMenuOpen;
        set => m_mobileMenuOpen = value;
    }
    
    protected string SearchQuery
    {
        get => m_searchQuery;
        set => m_searchQuery = value;
    }

    #endregion

    #region Injected Dependencies

    [Inject]
    public ConfigService ConfigService { get; set; } = null!;
    
    [Inject]
    public ContentService ContentService { get; set; } = null!;
    
    [Inject]
    public ThemeService ThemeService { get; set; } = null!;
    
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    #endregion
}
