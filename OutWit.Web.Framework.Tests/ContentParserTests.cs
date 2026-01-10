using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.Tests;

/// <summary>
/// Tests for extracting components with [[Component param="value"]] syntax.
/// </summary>
[TestFixture]
public class ComponentExtractionTest
{
    private ContentParser m_parser = null!;

    [SetUp]
    public void Setup()
    {
        m_parser = new ContentParser();
    }

    [Test]
    public void ExtractSelfClosingComponentTest()
    {
        // Arrange
        var input = """[[YouTube videoId="abc123"]]""";
        
        // Act
        var components = m_parser.ExtractComponents(input);
        
        // Assert
        Assert.That(components, Has.Count.EqualTo(1));
        Assert.That(components[0].Type, Is.EqualTo("YouTube"));
        Assert.That(components[0].Parameters["videoId"], Is.EqualTo("abc123"));
        Assert.That(components[0].InnerContent, Is.Null);
    }

    [Test]
    public void ExtractBlockComponentWithContentTest()
    {
        // Arrange
        var input = """
            [[FloatingImage src="./photo.jpg" position="right"]]
            
            ## Hello
            
            This text wraps around the image.
            
            [[/FloatingImage]]
            """;
        
        // Act
        var components = m_parser.ExtractComponents(input);
        
        // Assert
        Assert.That(components, Has.Count.EqualTo(1));
        Assert.That(components[0].Type, Is.EqualTo("FloatingImage"));
        Assert.That(components[0].Parameters["src"], Is.EqualTo("./photo.jpg"));
        Assert.That(components[0].Parameters["position"], Is.EqualTo("right"));
        Assert.That(components[0].InnerContent, Does.Contain("## Hello"));
        Assert.That(components[0].InnerContent, Does.Contain("wraps around"));
    }

    [Test]
    public void ExtractMultipleComponentsTest()
    {
        // Arrange
        var input = """
            # Title
            
            [[YouTube videoId="video1"]]
            
            Some text
            
            [[PowerPoint file="slides.pptx" width="800"]]
            
            More text
            
            [[Model3D src="./model.glb"]]
            """;
        
        // Act
        var components = m_parser.ExtractComponents(input);
        
        // Assert
        Assert.That(components, Has.Count.EqualTo(3));
        Assert.That(components[0].Type, Is.EqualTo("YouTube"));
        Assert.That(components[1].Type, Is.EqualTo("PowerPoint"));
        Assert.That(components[2].Type, Is.EqualTo("Model3D"));
    }

    [Test]
    public void ExtractMixedBlockAndSelfClosingTest()
    {
        // Arrange
        var input = """
            [[FloatingImage src="./img.jpg"]]
            Content inside
            [[/FloatingImage]]
            
            [[YouTube videoId="xyz789"]]
            """;
        
        // Act
        var components = m_parser.ExtractComponents(input);
        
        // Assert
        Assert.That(components, Has.Count.EqualTo(2));
        Assert.That(components[0].Type, Is.EqualTo("FloatingImage"));
        Assert.That(components[0].InnerContent, Does.Contain("Content inside"));
        Assert.That(components[1].Type, Is.EqualTo("YouTube"));
    }

    [Test]
    public void NoComponentsReturnsEmptyListTest()
    {
        // Arrange
        var input = "Just regular markdown content without any components.";
        
        // Act
        var components = m_parser.ExtractComponents(input);
        
        // Assert
        Assert.That(components, Is.Empty);
    }
}

/// <summary>
/// Tests for parameter parsing from component strings.
/// </summary>
[TestFixture]
public class ParameterParsingTest
{
    [Test]
    public void ParseDoubleQuotedParametersTest()
    {
        // Arrange
        var paramsStr = """videoId="abc123" playlistId="PLGBE0i"  """;
        
        // Act
        var result = ContentParser.ParseParameters(paramsStr);
        
        // Assert
        Assert.That(result["videoId"], Is.EqualTo("abc123"));
        Assert.That(result["playlistId"], Is.EqualTo("PLGBE0i"));
    }

    [Test]
    public void ParseSingleQuotedParametersTest()
    {
        // Arrange
        var paramsStr = "src='./photo.jpg' alt='My Photo'";
        
        // Act
        var result = ContentParser.ParseParameters(paramsStr);
        
        // Assert
        Assert.That(result["src"], Is.EqualTo("./photo.jpg"));
        Assert.That(result["alt"], Is.EqualTo("My Photo"));
    }

    [Test]
    public void ParseUnquotedParametersTest()
    {
        // Arrange
        var paramsStr = "width=350 height=200";
        
        // Act
        var result = ContentParser.ParseParameters(paramsStr);
        
        // Assert
        Assert.That(result["width"], Is.EqualTo("350"));
        Assert.That(result["height"], Is.EqualTo("200"));
    }

    [Test]
    public void ParseMixedQuoteStylesTest()
    {
        // Arrange
        var paramsStr = """src="file.jpg" alt='Description' width=500""";
        
        // Act
        var result = ContentParser.ParseParameters(paramsStr);
        
        // Assert
        Assert.That(result["src"], Is.EqualTo("file.jpg"));
        Assert.That(result["alt"], Is.EqualTo("Description"));
        Assert.That(result["width"], Is.EqualTo("500"));
    }

    [Test]
    public void ParseEmptyStringReturnsEmptyDictionaryTest()
    {
        // Act
        var result = ContentParser.ParseParameters("");
        
        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParameterNamesAreCaseInsensitiveTest()
    {
        // Arrange
        var paramsStr = """VideoId="abc" PLAYLISTID="xyz" """;
        
        // Act
        var result = ContentParser.ParseParameters(paramsStr);
        
        // Assert
        Assert.That(result["videoid"], Is.EqualTo("abc"));
        Assert.That(result["PlaylistId"], Is.EqualTo("xyz"));
    }
}

/// <summary>
/// Tests for placeholder replacement.
/// </summary>
[TestFixture]
public class PlaceholderReplacementTest
{
    private ContentParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new ContentParser();
    }

    [Test]
    public void ReplaceWithPlaceholdersTest()
    {
        // Arrange
        var input = """
            # Title
            
            [[YouTube videoId="abc"]]
            
            Some text
            """;
        
        // Act
        var components = _parser.ExtractComponents(input);
        var result = _parser.ReplaceWithPlaceholders(input, components);
        
        // Assert
        Assert.That(result, Does.Contain("<!--component:comp_0-->"));
        Assert.That(result, Does.Not.Contain("[[YouTube"));
        Assert.That(result, Does.Contain("# Title"));
        Assert.That(result, Does.Contain("Some text"));
    }

    [Test]
    public void TransformReturnsContentAndComponentsTest()
    {
        // Arrange
        var input = """
            import Something from 'somewhere';
            
            # Title
            
            [[Gallery images="a,b,c"]]
            """;
        
        // Act
        var (content, components) = _parser.Transform(input);
        
        // Assert
        Assert.That(components, Has.Count.EqualTo(1));
        Assert.That(components[0].Type, Is.EqualTo("Gallery"));
        Assert.That(content, Does.Not.Contain("import"));
        Assert.That(content, Does.Contain("<!--component:comp_0-->"));
    }
}

/// <summary>
/// Tests for import statement removal.
/// </summary>
[TestFixture]
public class ImportRemovalTest
{
    private ContentParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new ContentParser();
    }

    [Test]
    public void RemoveSingleImportStatementTest()
    {
        // Arrange
        var input = """
            import YouTube from '~/components/YouTube.astro';
            
            ## Content
            """;
        
        // Act
        var result = _parser.RemoveImportStatements(input);
        
        // Assert
        Assert.That(result, Does.Not.Contain("import"));
        Assert.That(result, Does.Contain("## Content"));
    }

    [Test]
    public void RemoveMultipleImportStatementsTest()
    {
        // Arrange
        var input = """
            import YouTube from '~/components/YouTube.astro';
            import ImageFloat from '~/components/ImageFloat.astro';
            import myPhoto from './avatar.jpg';
            
            ## Content
            """;
        
        // Act
        var result = _parser.RemoveImportStatements(input);
        
        // Assert
        Assert.That(result, Does.Not.Contain("import"));
        Assert.That(result, Does.Contain("## Content"));
    }
}
