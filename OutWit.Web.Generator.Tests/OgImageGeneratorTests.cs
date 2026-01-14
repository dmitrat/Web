using NUnit.Framework;
using OutWit.Web.Generator.Commands;
using OutWit.Web.Generator.Services;

namespace OutWit.Web.Generator.Tests;

/// <summary>
/// Tests for OgImageGenerator service.
/// Unit tests for helper methods that don't require Playwright.
/// </summary>
[TestFixture]
public class OgImageGeneratorTests
{
    #region CreateOgImageHtml Tests

    [Test]
    public void CreateOgImageHtmlGeneratesValidHtmlTest()
    {
        // Arrange
        var config = new GeneratorConfig { OutputPath = Path.GetTempPath() };
        var generator = new TestableOgImageGenerator(config, "https://example.com", "Test Site");

        // Act
        var html = generator.TestCreateOgImageHtml("Blog", "Test Title", "Test description for the OG image", "/blog/test");

        // Assert
        Assert.That(html, Does.Contain("<!DOCTYPE html>"));
        Assert.That(html, Does.Contain("Test Site"));
        Assert.That(html, Does.Contain("Test Title"));
        Assert.That(html, Does.Contain("Test description"));
        Assert.That(html, Does.Contain("Blog")); // Content type
        Assert.That(html, Does.Contain("/blog/test")); // URL in footer
        Assert.That(html, Does.Contain("1200px")); // OG image width
        Assert.That(html, Does.Contain("630px")); // OG image height
    }

    [Test]
    public void CreateOgImageHtmlEscapesHtmlCharactersTest()
    {
        // Arrange
        var config = new GeneratorConfig { OutputPath = Path.GetTempPath() };
        var generator = new TestableOgImageGenerator(config, "https://example.com", "Test <Site>");

        // Act
        var html = generator.TestCreateOgImageHtml("Article", "<script>alert('XSS')</script>", "A & B > C", "/article/test");

        // Assert
        Assert.That(html, Does.Contain("&lt;script&gt;"));
        Assert.That(html, Does.Contain("&amp;"));
        Assert.That(html, Does.Not.Contain("<script>alert"));
    }

    [Test]
    public void CreateOgImageHtmlTruncatesLongDescriptionTest()
    {
        // Arrange
        var config = new GeneratorConfig { OutputPath = Path.GetTempPath() };
        var generator = new TestableOgImageGenerator(config, "https://example.com", "Test Site");
        var longDescription = new string('A', 300);

        // Act
        var html = generator.TestCreateOgImageHtml("Project", "Title", longDescription, "/project/test");

        // Assert
        Assert.That(html, Does.Contain("...")); // Should be truncated
        Assert.That(html, Does.Not.Contain(longDescription)); // Full text should not be present
    }

    [Test]
    public void CreateOgImageHtmlHandlesEmptyDescriptionTest()
    {
        // Arrange
        var config = new GeneratorConfig { OutputPath = Path.GetTempPath() };
        var generator = new TestableOgImageGenerator(config, "https://example.com", "Test Site");

        // Act
        var html = generator.TestCreateOgImageHtml("Documentation", "Title Only", "", "/docs/test");

        // Assert
        Assert.That(html, Does.Contain("Title Only"));
        Assert.That(html, Does.Not.Contain("class=\"description\">")); // Description div should be empty/hidden
    }

    [Test]
    public void CreateOgImageHtmlIncludesFooterWithSiteNameAndUrlTest()
    {
        // Arrange
        var config = new GeneratorConfig { OutputPath = Path.GetTempPath() };
        var generator = new TestableOgImageGenerator(config, "https://example.com", "MySite");

        // Act
        var html = generator.TestCreateOgImageHtml("Blog", "Title", "Desc", "/blog/my-post");

        // Assert
        Assert.That(html, Does.Contain("class=\"site-name\""));
        Assert.That(html, Does.Contain("MySite"));
        Assert.That(html, Does.Contain("class=\"url\""));
        Assert.That(html, Does.Contain("/blog/my-post"));
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Testable wrapper for OgImageGenerator to expose internal methods.
    /// Does not initialize Playwright, only used for testing HTML generation.
    /// </summary>
    private class TestableOgImageGenerator : OgImageGenerator
    {
        public TestableOgImageGenerator(GeneratorConfig config, string siteUrl, string siteName)
            : base(config, siteUrl, siteName)
        {
        }

        public string TestCreateOgImageHtml(string contentType, string title, string description, string url)
        {
            return base.CreateOgImageHtml(contentType, title, description, url);
        }
    }

    #endregion
}
