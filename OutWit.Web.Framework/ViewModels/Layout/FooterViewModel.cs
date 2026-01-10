using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Configuration;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Layout;

public class FooterViewModel : ViewModelBase
{
    #region Fields

    private SiteConfig? m_config;

    #endregion

    #region Initialization

    protected override async Task OnInitializedAsync()
    {
        m_config = await ConfigService.GetConfigAsync();
    }

    #endregion

    #region Properties

    protected SiteConfig? Config => m_config;

    #endregion

    #region Injected Dependencies

    [Inject]
    public ConfigService ConfigService { get; set; } = null!;

    #endregion
}
