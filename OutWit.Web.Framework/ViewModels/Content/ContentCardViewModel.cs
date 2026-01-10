using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Content;

public class ContentCardViewModel : ViewModelBase
{
    #region Parameters

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;
    
    [Parameter, EditorRequired]
    public string Url { get; set; } = string.Empty;
    
    [Parameter]
    public MarkupString? Description { get; set; }
    
    [Parameter]
    public List<string>? Tags { get; set; }
    
    [Parameter]
    public bool ShowDate { get; set; }
    
    [Parameter]
    public DateTime? Date { get; set; }
    
    [Parameter]
    public bool ShowReadingTime { get; set; }
    
    [Parameter]
    public int ReadingTimeMinutes { get; set; }
    
    [Parameter]
    public string? TypeLabel { get; set; }
    
    [Parameter]
    public int MaxTags { get; set; } = 4;
    
    [Parameter]
    public string? Id { get; set; }

    #endregion
}
