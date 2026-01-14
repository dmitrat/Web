using NUnit.Framework;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for HostingConfigGenerator service.
/// </summary>
[TestFixture]
public class HostingConfigGeneratorTests
{
    #region Tests

    [Test]
    public async Task GenerateAsyncCloudflareCreatesHeadersAndRedirectsTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig
        {
            OutputPath = tempDir,
            HostingProvider = "cloudflare"
        };
        var generator = new HostingConfigGenerator(config);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert
            Assert.That(File.Exists(Path.Combine(tempDir, "_headers")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "_redirects")), Is.True);

            var headers = await File.ReadAllTextAsync(Path.Combine(tempDir, "_headers"));
            Assert.That(headers, Does.Contain("Cache-Control"));
            
            var redirects = await File.ReadAllTextAsync(Path.Combine(tempDir, "_redirects"));
            Assert.That(redirects, Does.Contain("/*  /index.html  200"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncNetlifyCreatesHeadersAndRedirectsTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig
        {
            OutputPath = tempDir,
            HostingProvider = "netlify"
        };
        var generator = new HostingConfigGenerator(config);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert - Netlify now uses _headers and _redirects (synced with PS)
            Assert.That(File.Exists(Path.Combine(tempDir, "_headers")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "_redirects")), Is.True);

            var headers = await File.ReadAllTextAsync(Path.Combine(tempDir, "_headers"));
            Assert.That(headers, Does.Contain("Cache-Control"));
            
            var redirects = await File.ReadAllTextAsync(Path.Combine(tempDir, "_redirects"));
            Assert.That(redirects, Does.Contain("/*  /index.html  200"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncVercelCreatesJsonTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig
        {
            OutputPath = tempDir,
            HostingProvider = "vercel"
        };
        var generator = new HostingConfigGenerator(config);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert
            var vercelJsonPath = Path.Combine(tempDir, "vercel.json");
            Assert.That(File.Exists(vercelJsonPath), Is.True);

            var content = await File.ReadAllTextAsync(vercelJsonPath);
            Assert.That(content, Does.Contain("rewrites"));
            Assert.That(content, Does.Contain("headers"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncGithubCreates404AndNojekyllTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        var config = new GeneratorConfig
        {
            OutputPath = tempDir,
            HostingProvider = "github"
        };
        var generator = new HostingConfigGenerator(config);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert
            Assert.That(File.Exists(Path.Combine(tempDir, ".nojekyll")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "404.html")), Is.True);

            // 404.html is a redirect page for SPA routing
            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "404.html"));
            Assert.That(content, Does.Contain("Redirecting"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncNoneCreatesNothingTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig
        {
            OutputPath = tempDir,
            HostingProvider = "none"
        };
        var generator = new HostingConfigGenerator(config);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert - no hosting files should be created
            Assert.That(File.Exists(Path.Combine(tempDir, "_headers")), Is.False);
            Assert.That(File.Exists(Path.Combine(tempDir, "_redirects")), Is.False);
            Assert.That(File.Exists(Path.Combine(tempDir, "vercel.json")), Is.False);
            Assert.That(File.Exists(Path.Combine(tempDir, ".nojekyll")), Is.False);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    #endregion

    #region Tools

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "OutWit.Test." + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    #endregion
}
