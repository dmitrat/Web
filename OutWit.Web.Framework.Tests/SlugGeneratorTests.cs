using OutWit.Web.Framework.Utils;

namespace OutWit.Web.Framework.Tests;

/// <summary>
/// Tests for SlugGenerator utility class.
/// </summary>
[TestFixture]
public class SlugGeneratorTests
{
    #region GenerateSlug Tests

    [Test]
    public void GenerateSlug_SimpleText_ReturnsLowercase()
    {
        // Arrange
        var input = "Hello World";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void GenerateSlug_WithSpecialCharacters_RemovesThem()
    {
        // Arrange
        var input = "What's New in C# 12?";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Is.EqualTo("whats-new-in-c-12"));
    }

    [Test]
    public void GenerateSlug_WithDotNetVersion_PreservesDot()
    {
        // Arrange
        var input = "Migrating to .NET 8";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        // Space before .NET becomes hyphen, then hyphen before dot is removed
        Assert.That(result, Is.EqualTo("migrating-to.net-8"));
    }

    [Test]
    public void GenerateSlug_WithUnderscores_ReplacesWithHyphens()
    {
        // Arrange
        var input = "hello_world_test";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world-test"));
    }

    [Test]
    public void GenerateSlug_WithMultipleSpaces_CollapsesToSingleHyphen()
    {
        // Arrange
        var input = "Hello    World";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void GenerateSlug_WithLeadingTrailingHyphens_TrimsThem()
    {
        // Arrange
        var input = "  Hello World  ";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void GenerateSlug_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = SlugGenerator.GenerateSlug("");
        
        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GenerateSlug_NullString_ReturnsEmpty()
    {
        // Act
        var result = SlugGenerator.GenerateSlug(null!);
        
        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GenerateSlug_VsNativeFormat_RemovesHyphenAroundDot()
    {
        // Arrange - Markdig produces "vs.native" not "vs.-native"
        var input = "C# vs. Native C++";
        
        // Act
        var result = SlugGenerator.GenerateSlug(input);
        
        // Assert
        Assert.That(result, Does.Not.Contain(".-"));
        Assert.That(result, Does.Not.Contain("-."));
    }

    #endregion

    #region GetSlugFromFilename Tests

    [Test]
    public void GetSlugFromFilename_SimpleMarkdown_ReturnsSlug()
    {
        // Arrange
        var filename = "hello-world.md";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void GetSlugFromFilename_WithMdxExtension_ReturnsSlug()
    {
        // Arrange
        var filename = "hello-world.mdx";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void GetSlugFromFilename_WithDatePrefix_RemovesDate()
    {
        // Arrange
        var filename = "2024-11-20-my-blog-post.md";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("my-blog-post"));
    }

    [Test]
    public void GetSlugFromFilename_WithOrderPrefix_RemovesOrder()
    {
        // Arrange
        var filename = "01-biography.md";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("biography"));
    }

    [Test]
    public void GetSlugFromFilename_FolderBasedContent_UsesFolderName()
    {
        // Arrange
        var filename = "01-biography/index.md";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("biography"));
    }

    [Test]
    public void GetSlugFromFilename_FolderWithMdxIndex_UsesFolderName()
    {
        // Arrange
        var filename = "02-project/index.mdx";
        
        // Act
        var result = SlugGenerator.GetSlugFromFilename(filename);
        
        // Assert
        Assert.That(result, Is.EqualTo("project"));
    }

    #endregion

    #region GetOrderAndSlugFromFilename Tests

    [Test]
    public void GetOrderAndSlugFromFilename_WithOrder_ReturnsCorrectTuple()
    {
        // Arrange
        var filename = "05-my-project.md";
        
        // Act
        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        
        // Assert
        Assert.That(order, Is.EqualTo(5));
        Assert.That(slug, Is.EqualTo("my-project"));
    }

    [Test]
    public void GetOrderAndSlugFromFilename_WithoutOrder_ReturnsZeroOrder()
    {
        // Arrange
        var filename = "my-project.md";
        
        // Act
        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        
        // Assert
        Assert.That(order, Is.EqualTo(0));
        Assert.That(slug, Is.EqualTo("my-project"));
    }

    [Test]
    public void GetOrderAndSlugFromFilename_FolderBased_ExtractsFromFolder()
    {
        // Arrange
        var filename = "03-witrpc/index.md";
        
        // Act
        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        
        // Assert
        Assert.That(order, Is.EqualTo(3));
        Assert.That(slug, Is.EqualTo("witrpc"));
    }

    [Test]
    public void GetOrderAndSlugFromFilename_OrderTen_ParsesCorrectly()
    {
        // Arrange
        var filename = "10-tenth-item.md";
        
        // Act
        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        
        // Assert
        Assert.That(order, Is.EqualTo(10));
        Assert.That(slug, Is.EqualTo("tenth-item"));
    }

    [Test]
    public void GetOrderAndSlugFromFilename_DatePrefix_DoesNotExtractAsOrder()
    {
        // Arrange - date prefix looks different from order prefix
        var filename = "2024-01-15-blog-post.md";
        
        // Act
        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        
        // Assert
        // Date prefix is not a 2-digit order prefix, so order should be 0
        // and slug should still have date in it since GetOrderAndSlugFromFilename
        // doesn't strip dates (that's what GetSlugFromFilename does)
        Assert.That(order, Is.EqualTo(0));
    }

    #endregion
}
