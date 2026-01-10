using Microsoft.JSInterop;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Service for managing theme (dark/light mode) with persistence.
/// </summary>
public class ThemeService
{
    #region Events

    public event Action<ThemeMode>? ThemeChanged;

    #endregion

    #region Constructors

    public ThemeService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Initialize theme from localStorage or system preference.
    /// </summary>
    public async Task InitializeAsync(string? defaultTheme)
    {
        if (string.IsNullOrEmpty(defaultTheme))
            await InitializeAsync(ThemeMode.Dark);
        else
            await InitializeAsync(ThemeMode.TryParse(defaultTheme, out var theme)
                ? theme!
                : ThemeMode.Dark);
    }

    /// <summary>
    /// Initialize theme from localStorage or system preference.
    /// </summary>
    public async Task InitializeAsync(ThemeMode defaultTheme)
    {
        try
        {
            var storedTheme = await JsRuntime.InvokeAsync<string?>("localStorage.getItem", "theme");
            
            if (!string.IsNullOrEmpty(storedTheme) && ThemeMode.TryParse(storedTheme, out var theme))
                CurrentTheme = theme!;
            else
            {
                // Check system preference using safe JS function
                var prefersDark = await JsRuntime.InvokeAsync<bool>("outwit.getSystemThemePreference");
                CurrentTheme = prefersDark 
                    ? ThemeMode.Dark
                    : defaultTheme;
            }

            await ApplyThemeAsync();
        }
        catch
        {
            // SSR or JS not available, use default
            CurrentTheme = defaultTheme;
        }
    }

    /// <summary>
    /// Toggle between dark and light themes.
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        CurrentTheme = CurrentTheme == ThemeMode.Dark
            ? ThemeMode.Light
            : ThemeMode.Dark;
        
        await SaveAndApplyThemeAsync();
    }

    /// <summary>
    /// Set a specific theme.
    /// </summary>
    public async Task SetThemeAsync(ThemeMode theme)
    {
        CurrentTheme = theme;
        
        await SaveAndApplyThemeAsync();
    }

    private async Task SaveAndApplyThemeAsync()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", CurrentTheme);
            await ApplyThemeAsync();
            ThemeChanged?.Invoke(CurrentTheme);
        }
        catch
        {
            // JS not available
        }
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            // Use safe JS function instead of eval
            await JsRuntime.InvokeVoidAsync("outwit.setThemeAttribute", CurrentTheme.ToString().ToLowerInvariant());
        }
        catch
        {
            // JS not available
        }
    }

    #endregion

    #region Properties
    
    private IJSRuntime JsRuntime { get; }
    
    /// <summary>
    /// Get current theme.
    /// </summary>
    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Dark;

    /// <summary>
    /// Check if dark mode is active.
    /// </summary>
    public bool IsDarkMode => CurrentTheme == ThemeMode.Dark;

    #endregion
}
