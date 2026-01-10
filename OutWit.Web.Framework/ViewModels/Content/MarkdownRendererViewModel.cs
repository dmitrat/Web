using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Content;

public class MarkdownRendererViewModel : ViewModelBase
{
    #region Fields

    private string m_htmlContent = string.Empty;

    #endregion

    #region Initialization

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(Content))
        {
            m_htmlContent = MarkdownService.ToHtml(Content);
        }
    }

    #endregion

    #region Properties

    protected string HtmlContent => m_htmlContent;

    #endregion

    #region Parameters

    [Parameter]
    public string Content { get; set; } = string.Empty;

    #endregion

    #region Injected Dependencies

    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    #endregion
}
