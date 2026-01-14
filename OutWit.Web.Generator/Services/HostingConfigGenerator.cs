using OutWit.Web.Generator.Commands;

namespace OutWit.Web.Generator.Services;

/// <summary>
/// Generates hosting provider configuration files.
/// </summary>
public class HostingConfigGenerator
{
    #region Fields

    private readonly GeneratorConfig m_config;

    #endregion

    #region Constructors

    public HostingConfigGenerator(GeneratorConfig config)
    {
        m_config = config;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Generate hosting provider specific configuration files.
    /// </summary>
    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        switch (m_config.HostingProvider.ToLowerInvariant())
        {
            case "cloudflare":
                await GenerateCloudflareConfigAsync(cancellationToken);
                break;
            case "netlify":
                await GenerateNetlifyConfigAsync(cancellationToken);
                break;
            case "vercel":
                await GenerateVercelConfigAsync(cancellationToken);
                break;
            case "github":
                await GenerateGithubPagesConfigAsync(cancellationToken);
                break;
        }
    }

    #endregion

    #region Hosting Providers

    private async Task GenerateCloudflareConfigAsync(CancellationToken cancellationToken)
    {
        // _headers file (synced with PS version)
        var headersContent = """
            # Cloudflare Pages headers
            # https://developers.cloudflare.com/pages/platform/headers

            # Cache static assets
            /_framework/*
              Cache-Control: public, max-age=31536000, immutable

            /css/*
              Cache-Control: public, max-age=31536000, immutable

            /images/*
              Cache-Control: public, max-age=86400

            # HTML pages - allow revalidation
            /*.html
              Cache-Control: public, max-age=0, must-revalidate

            /*/index.html
              Cache-Control: public, max-age=0, must-revalidate

            # Security headers
            /*
              X-Content-Type-Options: nosniff
              X-Frame-Options: DENY
              Referrer-Policy: strict-origin-when-cross-origin
            """;

        var headersPath = Path.Combine(m_config.OutputPath, "_headers");
        await File.WriteAllTextAsync(headersPath, headersContent, cancellationToken);
        Console.WriteLine($"  Created: {headersPath}");

        // _redirects file (SPA fallback like PS version)
        var redirectsContent = """
            # Cloudflare Pages redirects
            # SPA fallback - serve index.html for all routes not matching a file
            /*  /index.html  200
            """;

        var redirectsPath = Path.Combine(m_config.OutputPath, "_redirects");
        await File.WriteAllTextAsync(redirectsPath, redirectsContent, cancellationToken);
        Console.WriteLine($"  Created: {redirectsPath}");
    }

    private async Task GenerateNetlifyConfigAsync(CancellationToken cancellationToken)
    {
        // _headers file (synced with PS version)
        var headersContent = """
            # Netlify headers
            # https://docs.netlify.com/routing/headers/

            _framework/*
              Cache-Control: public, max-age=31536000, immutable

            /css/*
              Cache-Control: public, max-age=31536000, immutable

            /images/*
              Cache-Control: public, max-age=86400

            /*.html
              Cache-Control: public, max-age=0, must-revalidate

            /*
              X-Content-Type-Options: nosniff
              X-Frame-Options: DENY
              Referrer-Policy: strict-origin-when-cross-origin
            """;

        var headersPath = Path.Combine(m_config.OutputPath, "_headers");
        await File.WriteAllTextAsync(headersPath, headersContent, cancellationToken);
        Console.WriteLine($"  Created: {headersPath}");

        // _redirects file (SPA fallback)
        var redirectsContent = """
            # Netlify redirects
            # SPA fallback
            /*  /index.html  200
            """;

        var redirectsPath = Path.Combine(m_config.OutputPath, "_redirects");
        await File.WriteAllTextAsync(redirectsPath, redirectsContent, cancellationToken);
        Console.WriteLine($"  Created: {redirectsPath}");
    }

    private async Task GenerateVercelConfigAsync(CancellationToken cancellationToken)
    {
        var jsonContent = """
            {
              "rewrites": [
                { "source": "/((?!_framework|css|images|content|.*\\..*).*)", "destination": "/index.html" }
              ],
              "headers": [
                {
                  "source": "/_framework/(.*)",
                  "headers": [
                    { "key": "Cache-Control", "value": "public, max-age=31536000, immutable" }
                  ]
                },
                {
                  "source": "/css/(.*)",
                  "headers": [
                    { "key": "Cache-Control", "value": "public, max-age=31536000, immutable" }
                  ]
                },
                {
                  "source": "/images/(.*)",
                  "headers": [
                    { "key": "Cache-Control", "value": "public, max-age=86400" }
                  ]
                },
                {
                  "source": "/(.*)\\.html",
                  "headers": [
                    { "key": "Cache-Control", "value": "public, max-age=0, must-revalidate" }
                  ]
                }
              ]
            }
            """;

        var jsonPath = Path.Combine(m_config.OutputPath, "vercel.json");
        await File.WriteAllTextAsync(jsonPath, jsonContent, cancellationToken);
        Console.WriteLine($"  Created: {jsonPath}");
    }

    private async Task GenerateGithubPagesConfigAsync(CancellationToken cancellationToken)
    {
        // .nojekyll file to disable Jekyll processing
        var nojekyllPath = Path.Combine(m_config.OutputPath, ".nojekyll");
        await File.WriteAllTextAsync(nojekyllPath, "", cancellationToken);
        Console.WriteLine($"  Created: {nojekyllPath}");

        // 404.html for SPA routing (with sessionStorage like PS version)
        var html404Content = """
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>Redirecting...</title>
                <script>
                    // GitHub Pages SPA redirect
                    var path = window.location.pathname;
                    if (path !== '/' && path !== '/index.html') {
                        sessionStorage.setItem('redirectPath', path);
                        window.location.replace('/');
                    }
                </script>
            </head>
            <body>Redirecting...</body>
            </html>
            """;

        var html404Path = Path.Combine(m_config.OutputPath, "404.html");
        await File.WriteAllTextAsync(html404Path, html404Content, cancellationToken);
        Console.WriteLine($"  Created: {html404Path}");
    }

    #endregion
}
