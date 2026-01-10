using Microsoft.AspNetCore.Components;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Layout;

public class MainLayoutViewModel : LayoutComponentBase
{
    #region Initialization

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var config = await ConfigService.GetConfigAsync();
            
            await ThemeService.InitializeAsync(config.DefaultTheme);
            StateHasChanged();
        }
    }

    #endregion

    #region Injected Dependencies

    [Inject]
    public ConfigService ConfigService { get; set; } = null!;
    
    [Inject]
    public ThemeService ThemeService { get; set; } = null!;

    #endregion
}
