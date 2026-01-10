using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;
using Timer = System.Timers.Timer;

namespace OutWit.Web.Framework.ViewModels.Pages;

public class SearchPageViewModel : ViewModelBase, IDisposable
{
    #region Fields

    private List<SearchResult> m_results = [];
    private bool m_loading;
    private bool m_searched;
    private Timer? m_debounceTimer;
    private string? m_lastSearchedQuery;

    #endregion

    #region Initialization

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Query) && Query != m_lastSearchedQuery)
        {
            await PerformSearch();
        }
        else if (string.IsNullOrWhiteSpace(Query) && m_lastSearchedQuery != null)
        {
            m_results.Clear();
            m_searched = false;
            m_lastSearchedQuery = null;
        }
    }

    #endregion

    #region Functions

    protected void HandleKeyUp(KeyboardEventArgs e)
    {
        m_debounceTimer?.Stop();
        m_debounceTimer?.Dispose();
        
        m_debounceTimer = new Timer(300);
        m_debounceTimer.Elapsed += async (sender, args) =>
        {
            m_debounceTimer?.Stop();
            await InvokeAsync(async () =>
            {
                await PerformSearch();
                StateHasChanged();
            });
        };
        m_debounceTimer.AutoReset = false;
        m_debounceTimer.Start();
    }

    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            m_results.Clear();
            m_searched = false;
            m_lastSearchedQuery = null;
            return;
        }

        m_loading = true;
        StateHasChanged();

        try
        {
            m_results = await SearchService.SearchAsync(Query);
            m_searched = true;
            m_lastSearchedQuery = Query;
        }
        finally
        {
            m_loading = false;
        }
    }

    protected void ClearSearch()
    {
        Query = string.Empty;
        m_results.Clear();
        m_searched = false;
        QueryChanged.InvokeAsync(Query);
    }

    protected string GetTypeLabel(string type)
    {
        if (TypeLabels != null && TypeLabels.TryGetValue(type, out var label))
            return label;
            
        return type switch
        {
            "project" => "Project",
            "blog" => "Blog Post",
            "article" => "Article",
            _ => type
        };
    }

    private string RenderExcerpt(string text, List<string> terms)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var html = MarkdownService.ToHtml(text);
        
        if (terms.Count > 0)
        {
            foreach (var term in terms)
            {
                var pattern = Regex.Escape(term);
                html = Regex.Replace(
                    html, 
                    $@"(?<![\<\>])({pattern})(?![^\<]*\>)", 
                    m => $"<mark>{m.Value}</mark>", 
                    RegexOptions.IgnoreCase);
            }
        }
        
        return html;
    }
    
    protected MarkupString RenderExcerptMarkup(string text, List<string> terms)
    {
        return new MarkupString(RenderExcerpt(text, terms));
    }

    #endregion

    #region IDisposable

    protected override void Dispose(bool disposing)
    {

        m_debounceTimer?.Stop();
        m_debounceTimer?.Dispose();
        base.Dispose(disposing);
    }

    #endregion

    #region Properties

    protected List<SearchResult> Results => m_results;
    
    protected bool Loading => m_loading;
    
    protected bool Searched => m_searched;

    #endregion

    #region Parameters

    [Parameter]
    public string? Query { get; set; }
    
    [Parameter]
    public EventCallback<string?> QueryChanged { get; set; }
    
    [Parameter]
    public string Title { get; set; } = "Search";
    
    [Parameter]
    public string Placeholder { get; set; } = "Search projects, articles, blog posts...";
    
    [Parameter]
    public string SeoTitle { get; set; } = "Search";
    
    [Parameter]
    public string SeoDescription { get; set; } = "Search across all content.";
    
    [Parameter]
    public string LoadingText { get; set; } = "Searching...";
    
    [Parameter]
    public string NoResultsHint { get; set; } = "Try different keywords or check spelling";
    
    [Parameter]
    public Dictionary<string, string>? TypeLabels { get; set; }

    #endregion

    #region Injected Dependencies

    [Inject]
    public SearchService SearchService { get; set; } = null!;
    
    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    #endregion
}
