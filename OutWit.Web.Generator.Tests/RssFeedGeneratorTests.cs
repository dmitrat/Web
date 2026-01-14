using NUnit.Framework;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Framework.Content;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for RssFeedGenerator service.
/// </summary>
[TestFixture]
public class RssFeedGeneratorTests
{
    #region Tests

    [Test]
    public async Task GenerateAsyncCreatesValidRssFeedTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var contentDir = Path.Combine(tempDir, "content");
        var blogDir = Path.Combine(contentDir, "blog");
        Directory.CreateDirectory(blogDir);

        File.WriteAllText(Path.Combine(blogDir, "2024-01-15-test-post.md"), """
            ---
            title: Test Blog Post
            description: This is a test post
            publishDate: 2024-01-15
            ---
            
            # Content
            This is the blog post content.
            """);

        var config = new GeneratorConfig
        {
            OutputPath = tempDir
        };
        var generator = new RssFeedGenerator(config, "https://example.com", "Test Site");
        var index = new ContentIndex
        {
            Blog = ["2024-01-15-test-post.md"]
        };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var feedPath = Path.Combine(tempDir, "feed.xml");
            Assert.That(File.Exists(feedPath), Is.True);

            var content = await File.ReadAllTextAsync(feedPath);
            Assert.That(content, Does.Contain("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
            Assert.That(content, Does.Contain("<rss version=\"2.0\""));
            Assert.That(content, Does.Contain("<title>Test Site</title>"));
            Assert.That(content, Does.Contain("<title>Test Blog Post</title>"));
            Assert.That(content, Does.Contain("<link>https://example.com/blog/test-post</link>"));
            Assert.That(content, Does.Contain("<description>This is a test post</description>"));
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task GenerateAsyncLimitsTo20PostsTest()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var contentDir = Path.Combine(tempDir, "content");
        var blogDir = Path.Combine(contentDir, "blog");
        Directory.CreateDirectory(blogDir);

        var blogFiles = new List<string>();
        for (int i = 1; i <= 25; i++)
        {
            var filename = $"2024-01-{i:D2}-post-{i}.md";
            File.WriteAllText(Path.Combine(blogDir, filename), $"---\ntitle: Post {i}\n---\nContent");
            blogFiles.Add(filename);
        }

        var config = new GeneratorConfig
        {
            OutputPath = tempDir
        };
        var generator = new RssFeedGenerator(config, "https://example.com", "Test Site");
        var index = new ContentIndex { Blog = blogFiles };

        try
        {
            // Act
            await generator.GenerateAsync(index);

            // Assert
            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "feed.xml"));
            var itemCount = content.Split("<item>").Length - 1;
            Assert.That(itemCount, Is.EqualTo(20));
        }
        finally
        {
            // Cleanup
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
