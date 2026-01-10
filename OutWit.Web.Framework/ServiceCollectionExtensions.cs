using Microsoft.Extensions.DependencyInjection;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework;

/// <summary>
/// Extension methods for registering OutWit.Web.Framework services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add OutWit.Web.Framework services to the DI container.
    /// </summary>
    public static IServiceCollection AddOutWitWebFramework(this IServiceCollection services)
    {
        services.AddScoped<MarkdownService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<ConfigService>();
        services.AddScoped<ContentService>();
        services.AddScoped<SearchService>();
        services.AddSingleton<ContentParser>();
        services.AddSingleton<ComponentRegistry>();
        
        return services;
    }
}
