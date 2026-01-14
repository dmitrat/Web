using NUnit.Framework;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for ContentHelpers utility class.
/// </summary>
[TestFixture]
public class ContentHelpersTests
{
    #region ExtractFrontmatter Tests

    [Test]
    public void ExtractFrontmatterParsesYamlCorrectlyTest()
    {
        // Arrange
        var markdown = """
            ---
            title: Test Title
            description: Test Description
            tags: [tag1, tag2]
            ---
            
            # Content
            This is the body.
            """;

        // Act
        var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

        // Assert
        Assert.That(frontmatter, Is.Not.Null);
        Assert.That(frontmatter!.Title, Is.EqualTo("Test Title"));
        Assert.That(frontmatter.Description, Is.EqualTo("Test Description"));
        Assert.That(frontmatter.Tags, Has.Count.EqualTo(2));
        Assert.That(content, Does.Contain("# Content"));
    }

    [Test]
    public void ExtractFrontmatterReturnsNullForNoFrontmatterTest()
    {
        // Arrange
        var markdown = "# Just Content\nNo frontmatter here.";

        // Act
        var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

        // Assert
        Assert.That(frontmatter, Is.Null);
        Assert.That(content, Is.EqualTo(markdown));
    }

    [Test]
    public void ExtractFrontmatterHandlesMalformedYamlTest()
    {
        // Arrange
        var markdown = """
            ---
            title: [invalid: yaml: format
            ---
            
            Content
            """;

        // Act
        var (frontmatter, content) = ContentHelpers.ExtractFrontmatter(markdown);

        // Assert - should return null for malformed YAML
        Assert.That(frontmatter, Is.Null);
    }

    #endregion

    #region GetSlugFromPath Tests

    [Test]
    public void GetSlugFromPathRemovesDatePrefixTest()
    {
        // Arrange
        var path = "2024-01-15-my-blog-post.md";

        // Act
        var slug = ContentHelpers.GetSlugFromPath(path);

        // Assert
        Assert.That(slug, Is.EqualTo("my-blog-post"));
    }

    [Test]
    public void GetSlugFromPathRemovesOrderPrefixTest()
    {
        // Arrange
        var path = "01-introduction.md";

        // Act
        var slug = ContentHelpers.GetSlugFromPath(path);

        // Assert
        Assert.That(slug, Is.EqualTo("introduction"));
    }

    [Test]
    public void GetSlugFromPathHandlesIndexMdTest()
    {
        // Arrange
        var path = Path.Combine("01-project", "index.md");

        // Act
        var slug = ContentHelpers.GetSlugFromPath(path);

        // Assert
        Assert.That(slug, Is.EqualTo("project"));
    }

    [Test]
    public void GetSlugFromPathHandlesSimpleFilenameTest()
    {
        // Arrange
        var path = "about.md";

        // Act
        var slug = ContentHelpers.GetSlugFromPath(path);

        // Assert
        Assert.That(slug, Is.EqualTo("about"));
    }

    #endregion

    #region EscapeHtml Tests

    [Test]
    public void EscapeHtmlEscapesSpecialCharactersTest()
    {
        // Arrange
        var text = "<script>alert('XSS')</script> & \"quotes\"";

        // Act
        var escaped = ContentHelpers.EscapeHtml(text);

        // Assert
        Assert.That(escaped, Does.Contain("&lt;"));
        Assert.That(escaped, Does.Contain("&gt;"));
        Assert.That(escaped, Does.Contain("&amp;"));
        Assert.That(escaped, Does.Contain("&quot;"));
        Assert.That(escaped, Does.Not.Contain("<script>"));
    }

    [Test]
    public void EscapeXmlEscapesApostropheTest()
    {
        // Arrange
        var text = "It's a test";

        // Act
        var escaped = ContentHelpers.EscapeXml(text);

        // Assert
        Assert.That(escaped, Does.Contain("&apos;"));
    }

    #endregion

    #region TruncateText Tests

    [Test]
    public void TruncateTextLimitsLengthTest()
    {
        // Arrange
        var text = "This is a long text that should be truncated because it exceeds the limit";

        // Act
        var truncated = ContentHelpers.TruncateText(text, 20);

        // Assert
        Assert.That(truncated, Has.Length.EqualTo(23)); // 20 + "..."
        Assert.That(truncated, Does.EndWith("..."));
    }

    [Test]
    public void TruncateTextReturnsSameIfShortTest()
    {
        // Arrange
        var text = "Short text";

        // Act
        var result = ContentHelpers.TruncateText(text, 100);

        // Assert
        Assert.That(result, Is.EqualTo(text));
    }

    #endregion

    #region ExtractPlainText Tests

    [Test]
    public void ExtractPlainTextRemovesMarkdownSyntaxTest()
    {
        // Arrange
        var markdown = "# Header\n\n**Bold** and *italic* and [link](url)";

        // Act
        var plainText = ContentHelpers.ExtractPlainText(markdown);

        // Assert
        Assert.That(plainText, Does.Not.Contain("#"));
        Assert.That(plainText, Does.Not.Contain("**"));
        Assert.That(plainText, Does.Not.Contain("["));
        Assert.That(plainText, Does.Contain("Header"));
        Assert.That(plainText, Does.Contain("Bold"));
    }

    #endregion
}
