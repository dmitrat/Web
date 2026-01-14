using NUnit.Framework;
using OutWit.Web.Framework.Content;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for SearchIndexGenerator service.
/// </summary>
[TestFixture]
public class SearchIndexGeneratorTests
{
    #region Tests

    [Test]
    public async Task GenerateAsyncCreatesSearchIndexJsonTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        SetupContentDirectory(tempDir, "blog", "2024-01-15-test-post.md", """
            ---
            title: Test Blog Post
            description: This is a test post for search
            tags: [testing, search]
            ---
            
            # Content
            This is searchable content about .NET development.
            """);

        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SearchIndexGenerator(config);
        var index = new ContentIndex { Blog = ["2024-01-15-test-post.md"] };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var searchIndexPath = Path.Combine(tempDir, "search-index.json");
            Assert.That(File.Exists(searchIndexPath), Is.True);

            var content = await File.ReadAllTextAsync(searchIndexPath);
            Assert.That(content, Does.Contain("Test Blog Post"));
            Assert.That(content, Does.Contain("blog/test-post"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncHandlesMultipleContentTypesTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        SetupContentDirectory(tempDir, "blog", "2024-01-15-post.md", "---\ntitle: Blog Post\n---\nBlog content");
        SetupContentDirectory(tempDir, "articles", "01-article.md", "---\ntitle: Article Title\n---\nArticle content");

        var config = new GeneratorConfig { OutputPath = tempDir };
        var generator = new SearchIndexGenerator(config);
        var index = new ContentIndex
        {
            Blog = ["2024-01-15-post.md"],
            Articles = ["01-article.md"]
        };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "search-index.json"));
            Assert.That(content, Does.Contain("blog/post"));
            Assert.That(content, Does.Contain("article/article"));
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

    private static void SetupContentDirectory(string rootDir, string folder, string filename, string content)
    {
        // Content is at rootDir/content/{folder}/{filename}
        var dir = Path.Combine(rootDir, "content", folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, filename), content);
    }

    #endregion
}
