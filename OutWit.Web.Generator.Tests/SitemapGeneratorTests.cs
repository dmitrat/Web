using NUnit.Framework;
using OutWit.Web.Framework.Content;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for SitemapGenerator service.
/// </summary>
[TestFixture]
public class SitemapGeneratorTests
{
    #region Tests

    [Test]
    public async Task GenerateAsyncCreatesValidSitemapXmlTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SitemapGenerator(config, "https://example.com");
        var index = new ContentIndex
        {
            Blog = ["2024-01-15-test-post.md"],
            Projects = ["01-project/index.md"],
            Articles = ["test-article.md"]
        };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var sitemapPath = Path.Combine(tempDir, "sitemap.xml");
            Assert.That(File.Exists(sitemapPath), Is.True);

            var content = await File.ReadAllTextAsync(sitemapPath);
            
            // Basic structure checks
            Assert.That(content, Does.Contain("<?xml"));
            Assert.That(content, Does.Contain("urlset"));
            Assert.That(content, Does.Contain("https://example.com"));
            
            // Content pages should be present
            Assert.That(content, Does.Contain("test-post"));
            Assert.That(content, Does.Contain("project"));
            Assert.That(content, Does.Contain("test-article"));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncCreatesRobotsTxtTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SitemapGenerator(config, "https://example.com");
        var index = new ContentIndex();

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var robotsPath = Path.Combine(tempDir, "robots.txt");
            Assert.That(File.Exists(robotsPath), Is.True);

            var content = await File.ReadAllTextAsync(robotsPath);
            Assert.That(content, Does.Contain("User-agent:"));
            Assert.That(content, Does.Contain("Allow:"));
            Assert.That(content, Does.Contain("Sitemap:"));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncTrimsTrailingSlashFromUrlTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SitemapGenerator(config, "https://example.com/");
        var index = new ContentIndex();

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "sitemap.xml"));
            
            // Should contain base URL without double slash
            Assert.That(content, Does.Contain("https://example.com"));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncIncludesDynamicSectionsTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SitemapGenerator(config, "https://example.com");
        var index = new ContentIndex
        {
            Blog = ["2024-01-15-test-post.md"],
            Sections = new Dictionary<string, List<string>>
            {
                ["use-cases"] = ["01-enterprise.md", "02-startup.md"],
                ["solutions"] = ["cloud.md"]
            }
        };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "sitemap.xml"));
            
            // Dynamic sections should be present
            Assert.That(content, Does.Contain("use-cases/enterprise"));
            Assert.That(content, Does.Contain("use-cases/startup"));
            Assert.That(content, Does.Contain("solutions/cloud"));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
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
