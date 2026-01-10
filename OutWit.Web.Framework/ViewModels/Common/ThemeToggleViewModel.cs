using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Common;

public class ThemeToggleViewModel: ViewModelBase
{
    #region Constructors

    public ThemeToggleViewModel()
    {
        IsDark = true;
    }

    #endregion

    #region Functions

    protected async Task ToggleTheme()
    {
        await ThemeService.ToggleThemeAsync();
    }

    #endregion

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        ThemeService.ThemeChanged -= OnThemeChanged;
        base.Dispose(disposing);
    }

    #endregion

    #region Event Handlers
    
    protected override void OnInitialized()
    {
        IsDark = ThemeService.IsDarkMode;
        ThemeService.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(ThemeMode theme)
    {
        IsDark = theme == ThemeMode.Dark;
        StateHasChanged();
    }

    #endregion

    #region Properties

    protected bool IsDark { get; private set; }

    #endregion
    
    #region Injected Dependencies

    [Inject]
    public ThemeService ThemeService { get; private set; } = null!;

    #endregion
}