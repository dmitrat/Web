using OutWit.Web.Framework.Services;
using OutWit.Web.Framework.Content;

namespace OutWit.Web.Framework.Tests;

/// <summary>
/// Tests for MarkdownService.
/// </summary>
[TestFixture]
public class MarkdownServiceTests
{
    private MarkdownService m_service = null!;

    [SetUp]
    public void Setup()
    {
        m_service = new MarkdownService();
    }

    #region ToHtml Tests

    [Test]
    public void ToHtml_SimpleText_ReturnsHtml()
    {
        // Arrange
        var markdown = "Hello **world**!";
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<strong>world</strong>"));
        Assert.That(html, Does.Contain("<p>"));
    }

    [Test]
    public void ToHtml_Headings_ReturnsHtmlWithIds()
    {
        // Arrange
        var markdown = "# Hello World\n\n## Section Two";
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<h1"));
        Assert.That(html, Does.Contain("<h2"));
        Assert.That(html, Does.Contain("id=")); // AutoIdentifiers extension
    }

    [Test]
    public void ToHtml_CodeBlock_PreservesCode()
    {
        // Arrange
        var markdown = """
            ```csharp
            var x = 1;
            ```
            """;
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<pre>"));
        Assert.That(html, Does.Contain("<code"));
        Assert.That(html, Does.Contain("var x = 1;"));
    }

    [Test]
    public void ToHtml_Links_CreatesAnchors()
    {
        // Arrange
        var markdown = "Visit [GitHub](https://github.com)";
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<a href=\"https://github.com\""));
        Assert.That(html, Does.Contain(">GitHub</a>"));
    }

    [Test]
    public void ToHtml_Lists_CreatesList()
    {
        // Arrange
        var markdown = "- Item 1\n- Item 2\n- Item 3";
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<ul>"));
        Assert.That(html, Does.Contain("<li>Item 1</li>"));
    }

    [Test]
    public void ToHtml_TaskList_CreatesCheckboxes()
    {
        // Arrange
        var markdown = "- [x] Done\n- [ ] Todo";
        
        // Act
        var html = m_service.ToHtml(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("type=\"checkbox\""));
        Assert.That(html, Does.Contain("checked"));
    }

    [Test]
    public void ToHtml_EmptyString_ReturnsEmpty()
    {
        // Act
        var html = m_service.ToHtml("");
        
        // Assert
        Assert.That(html, Is.EqualTo(""));
    }

    #endregion

    #region ParseWithFrontmatter Tests

    [Test]
    public void ParseWithFrontmatter_ValidYaml_ExtractsFrontmatter()
    {
        // Arrange
        var markdown = """
            ---
            title: Test Post
            description: A test description
            tags:
              - test
              - sample
            ---
            
            # Content Here
            
            Some text.
            """;
        
        // Act
        var (frontmatter, html) = m_service.ParseWithFrontmatter<FrontmatterData>(markdown);
        
        // Assert
        Assert.That(frontmatter, Is.Not.Null);
        Assert.That(frontmatter!.Title, Is.EqualTo("Test Post"));
        Assert.That(frontmatter.Description, Is.EqualTo("A test description"));
        Assert.That(frontmatter.Tags, Has.Count.EqualTo(2));
        Assert.That(frontmatter.Tags, Does.Contain("test"));
    }

    [Test]
    public void ParseWithFrontmatter_NoFrontmatter_ReturnsNullFrontmatter()
    {
        // Arrange
        var markdown = "# Just Content\n\nNo frontmatter here.";
        
        // Act
        var (frontmatter, html) = m_service.ParseWithFrontmatter<FrontmatterData>(markdown);
        
        // Assert
        Assert.That(frontmatter, Is.Null);
        Assert.That(html, Does.Contain("<h1"));
    }

    [Test]
    public void ParseWithFrontmatter_WithDate_ParsesDate()
    {
        // Arrange
        var markdown = """
            ---
            title: Blog Post
            publishDate: 2024-01-15
            ---
            
            Content
            """;
        
        // Act
        var (frontmatter, _) = m_service.ParseWithFrontmatter<FrontmatterData>(markdown);
        
        // Assert
        Assert.That(frontmatter, Is.Not.Null);
        Assert.That(frontmatter!.PublishDate, Is.EqualTo(new DateTime(2024, 1, 15)));
    }

    [Test]
    public void ParseWithFrontmatter_HtmlContainsContent()
    {
        // Arrange
        var markdown = """
            ---
            title: Test
            ---
            
            ## Hello
            
            Paragraph text.
            """;
        
        // Act
        var (_, html) = m_service.ParseWithFrontmatter<FrontmatterData>(markdown);
        
        // Assert
        Assert.That(html, Does.Contain("<h2"));
        Assert.That(html, Does.Contain("Paragraph text."));
    }

    #endregion

    #region CalculateReadingTime Tests

    [Test]
    public void CalculateReadingTime_ShortText_ReturnsOneMinute()
    {
        // Arrange
        var markdown = "Hello world."; // 2 words
        
        // Act
        var time = m_service.CalculateReadingTime(markdown);
        
        // Assert
        Assert.That(time, Is.EqualTo(1)); // Minimum 1 minute
    }

    [Test]
    public void CalculateReadingTime_LongText_CalculatesCorrectly()
    {
        // Arrange - 400 words at 200 wpm = 2 minutes
        var words = string.Join(" ", Enumerable.Repeat("word", 400));
        
        // Act
        var time = m_service.CalculateReadingTime(words, 200);
        
        // Assert
        Assert.That(time, Is.EqualTo(2));
    }

    [Test]
    public void CalculateReadingTime_WithMarkdown_StripsFormatting()
    {
        // Arrange
        var markdown = "**Bold** and *italic* and `code` with [link](url)";
        
        // Act
        var time = m_service.CalculateReadingTime(markdown);
        
        // Assert
        // Should still calculate based on actual words
        Assert.That(time, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void CalculateReadingTime_CustomWordsPerMinute()
    {
        // Arrange - 300 words at 100 wpm = 3 minutes
        var words = string.Join(" ", Enumerable.Repeat("word", 300));
        
        // Act
        var time = m_service.CalculateReadingTime(words, 100);
        
        // Assert
        Assert.That(time, Is.EqualTo(3));
    }

    #endregion

    #region ExtractPlainText Tests

    [Test]
    public void ExtractPlainText_RemovesFrontmatter()
    {
        // Arrange
        var markdown = """
            ---
            title: Test
            ---
            
            Actual content here.
            """;
        
        // Act
        var text = m_service.ExtractPlainText(markdown);
        
        // Assert
        Assert.That(text, Does.Not.Contain("title:"));
        Assert.That(text, Does.Not.Contain("---"));
        Assert.That(text, Does.Contain("Actual content here"));
    }

    [Test]
    public void ExtractPlainText_RemovesMarkdownSyntax()
    {
        // Arrange
        var markdown = "**Bold** and *italic* and `code`";
        
        // Act
        var text = m_service.ExtractPlainText(markdown);
        
        // Assert
        Assert.That(text, Does.Not.Contain("**"));
        Assert.That(text, Does.Not.Contain("*"));
        Assert.That(text, Does.Not.Contain("`"));
    }

    [Test]
    public void ExtractPlainText_NormalizesWhitespace()
    {
        // Arrange
        var markdown = "Hello    world\n\n\ntest";
        
        // Act
        var text = m_service.ExtractPlainText(markdown);
        
        // Assert
        Assert.That(text, Does.Not.Contain("  ")); // No double spaces
    }

    #endregion

    #region ExtractTableOfContents Tests

    [Test]
    public void ExtractTableOfContents_SimpleHeadings_ReturnsToc()
    {
        // Arrange
        var markdown = """
            # Heading 1
            
            Content
            
            ## Heading 2
            
            More content
            
            ## Heading 3
            """;
        
        // Act
        var toc = m_service.ExtractTableOfContents(markdown);
        
        // Assert
        Assert.That(toc, Has.Count.EqualTo(1)); // One H1
        Assert.That(toc[0].Title, Is.EqualTo("Heading 1"));
        Assert.That(toc[0].Children, Has.Count.EqualTo(2)); // Two H2s under it
    }

    [Test]
    public void ExtractTableOfContents_GeneratesIds()
    {
        // Arrange
        var markdown = "# Hello World";
        
        // Act
        var toc = m_service.ExtractTableOfContents(markdown);
        
        // Assert
        Assert.That(toc, Has.Count.EqualTo(1));
        Assert.That(toc[0].Id, Is.EqualTo("hello-world"));
    }

    [Test]
    public void ExtractTableOfContents_NoHeadings_ReturnsEmptyList()
    {
        // Arrange
        var markdown = "Just some text without any headings.";
        
        // Act
        var toc = m_service.ExtractTableOfContents(markdown);
        
        // Assert
        Assert.That(toc, Is.Empty);
    }

    [Test]
    public void ExtractTableOfContents_NestedHeadings_BuildsHierarchy()
    {
        // Arrange
        var markdown = """
            # Chapter 1
            ## Section 1.1
            ### Subsection 1.1.1
            ## Section 1.2
            # Chapter 2
            ## Section 2.1
            """;
        
        // Act
        var toc = m_service.ExtractTableOfContents(markdown);
        
        // Assert
        Assert.That(toc, Has.Count.EqualTo(2)); // Two H1s
        Assert.That(toc[0].Title, Is.EqualTo("Chapter 1"));
        Assert.That(toc[0].Children, Has.Count.EqualTo(2)); // Two H2s
        Assert.That(toc[0].Children[0].Children, Has.Count.EqualTo(1)); // One H3
        Assert.That(toc[1].Title, Is.EqualTo("Chapter 2"));
    }

    #endregion
}
