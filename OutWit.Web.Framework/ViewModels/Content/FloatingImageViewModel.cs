using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Content;

public class FloatingImageViewModel : ViewModelBase
{
    #region Functions

    protected string GetResolvedSrc()
    {
        if (Src.StartsWith("./") && !string.IsNullOrEmpty(BasePath))
        {
            return $"{BasePath.TrimStart('/')}/{Src[2..]}";
        }
        return Src;
    }
    
    protected string GetRenderedContent()
    {
        if (string.IsNullOrEmpty(InnerContent))
            return string.Empty;
        
        return MarkdownService.ToHtml(InnerContent);
    }

    #endregion

    #region Parameters

    [Parameter]
    public string Src { get; set; } = string.Empty;
    
    [Parameter]
    public string Alt { get; set; } = string.Empty;
    
    [Parameter]
    public string Width { get; set; } = "300";
    
    [Parameter]
    public string Position { get; set; } = "right";
    
    [Parameter]
    public string? InnerContent { get; set; }
    
    [Parameter]
    public string BasePath { get; set; } = string.Empty;

    #endregion

    #region Injected Dependencies

    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    #endregion
}
