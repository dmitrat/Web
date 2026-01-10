using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Content;

public class YouTubeViewModel : ViewModelBase
{
    #region Functions

    protected string GetEmbedHtml()
    {
        var url = $"https://www.youtube.com/embed/{VideoId}";
        if (!string.IsNullOrEmpty(PlaylistId))
        {
            url += $"?list={PlaylistId}";
        }
        
        return $"""
            <div class="youtube-embed">
                <iframe 
                    src="{url}"
                    title="{Title}"
                    frameborder="0"
                    loading="lazy"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                    allowfullscreen>
                </iframe>
            </div>
            """;
    }

    #endregion

    #region Parameters

    [Parameter]
    public string VideoId { get; set; } = string.Empty;
    
    [Parameter]
    public string? PlaylistId { get; set; }
    
    [Parameter]
    public string Title { get; set; } = "YouTube video";
    
    [Parameter]
    public string BasePath { get; set; } = string.Empty;

    #endregion
}
