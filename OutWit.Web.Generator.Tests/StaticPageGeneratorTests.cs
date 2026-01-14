using NUnit.Framework;
using OutWit.Web.Framework.Content;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for StaticPageGenerator service.
/// </summary>
[TestFixture]
public class StaticPageGeneratorTests
{
    #region Tests

    [Test]
    public async Task GenerateAsyncSkipsIfNoTemplateTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new StaticPageGenerator(config, "https://example.com", "Test Site");
        var index = new ContentIndex { Blog = ["test.md"] };

        try
        {
            // Act - should not throw, just skip
            await generator.GenerateAsync(index);

            // Assert - no pages generated without index.html template
            var blogDir = Path.Combine(tempDir, "blog");
            // blog dir may exist from index page but no post subdirectory
            Assert.That(Directory.Exists(Path.Combine(tempDir, "blog", "test")), Is.False);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncCreatesStaticHtmlPagesTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        // Create template
        CreateTemplate(tempDir);

        // Create blog post in content directory
        SetupContentDirectory(tempDir, "blog", "2024-01-15-test-post.md", """
            ---
            title: Test Blog Post
            description: Test description
            publishDate: 2024-01-15
            ---
            
            # Hello World
            This is test content.
            """);

        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new StaticPageGenerator(config, "https://example.com", "Test Site");
        var index = new ContentIndex { Blog = ["2024-01-15-test-post.md"] };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var outputPath = Path.Combine(tempDir, "blog", "test-post", "index.html");
            Assert.That(File.Exists(outputPath), Is.True);

            var content = await File.ReadAllTextAsync(outputPath);
            Assert.That(content, Does.Contain("Test Blog Post"));
            Assert.That(content, Does.Contain("og:title"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncGeneratesIndexPagesTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        CreateTemplate(tempDir);

        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new StaticPageGenerator(config, "https://example.com", "Test Site");
        var index = new ContentIndex();

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert - index pages should be created
            Assert.That(File.Exists(Path.Combine(tempDir, "blog", "index.html")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "contact", "index.html")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "search", "index.html")), Is.True);
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

    private static void CreateTemplate(string rootDir)
    {
        File.WriteAllText(Path.Combine(rootDir, "index.html"), """
            <!DOCTYPE html>
            <html>
            <head><title>Template</title></head>
            <body><div id="app">Loading...</div></body>
            </html>
            """);
    }

    private static void SetupContentDirectory(string rootDir, string folder, string filename, string content)
    {
        var dir = Path.Combine(rootDir, "content", folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, filename), content);
    }

    #endregion
}
