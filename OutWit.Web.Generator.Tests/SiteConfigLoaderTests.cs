using NUnit.Framework;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for SiteConfigLoader service.
/// </summary>
[TestFixture]
public class SiteConfigLoaderTests
{
    #region Tests

    [Test]
    public async Task LoadAsyncReturnsConfigFromValidJsonTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var configPath = Path.Combine(tempDir, "site.config.json");
        
        await File.WriteAllTextAsync(configPath, """
            {
              "siteName": "Test Site",
              "baseUrl": "https://example.com",
              "defaultTheme": "dark"
            }
            """);

        var loader = new SiteConfigLoader(configPath);

        try
        {
            // Act
            var config = await loader.LoadAsync();

            // Assert
            Assert.That(config, Is.Not.Null);
            Assert.That(config!.SiteName, Is.EqualTo("Test Site"));
            Assert.That(config.BaseUrl, Is.EqualTo("https://example.com"));
            Assert.That(config.DefaultTheme, Is.EqualTo("dark"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task LoadAsyncReturnsNullWhenFileNotFoundTest()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-config.json");
        var loader = new SiteConfigLoader(nonExistentPath);

        // Act
        var config = await loader.LoadAsync();

        // Assert
        Assert.That(config, Is.Null);
    }

    [Test]
    public async Task LoadAsyncReturnsNullOnInvalidJsonTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var configPath = Path.Combine(tempDir, "site.config.json");
        
        await File.WriteAllTextAsync(configPath, "{ invalid json content");

        var loader = new SiteConfigLoader(configPath);

        try
        {
            // Act
            var config = await loader.LoadAsync();

            // Assert
            Assert.That(config, Is.Null);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task LoadAsyncHandlesCaseInsensitivePropertiesTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var configPath = Path.Combine(tempDir, "site.config.json");
        
        await File.WriteAllTextAsync(configPath, """
            {
              "SiteName": "Test Site",
              "BASEURL": "https://example.com"
            }
            """);

        var loader = new SiteConfigLoader(configPath);

        try
        {
            // Act
            var config = await loader.LoadAsync();

            // Assert
            Assert.That(config, Is.Not.Null);
            Assert.That(config!.SiteName, Is.EqualTo("Test Site"));
            Assert.That(config.BaseUrl, Is.EqualTo("https://example.com"));
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
