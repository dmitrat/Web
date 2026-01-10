using System.Net.Http.Json;
using System.Text.RegularExpressions;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Utils;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Service for loading content from markdown files in wwwroot/content folder.
/// Enables dynamic content loading without hardcoding - just add .md files to the content folder.
/// </summary>
public class ContentService
{
    #region Fields

    // Cache for loaded content
    private List<BlogPost>? m_blogPosts;
    private List<ProjectCard>? m_projects;
    private List<ArticleCard>? m_articles;
    private List<FeatureCard>? m_features;
    private Dictionary<string, DocPage>? m_docs;
    private ContentIndex? m_contentIndex;
    
    // Async locks for thread-safe caching
    private readonly SemaphoreSlim m_projectsLock = new(1, 1);
    private readonly SemaphoreSlim m_blogPostsLock = new(1, 1);
    private readonly SemaphoreSlim m_articlesLock = new(1, 1);
    private readonly SemaphoreSlim m_featuresLock = new(1, 1);

    #endregion

    #region Constructors

    public ContentService(HttpClient http, MarkdownService markdownService, ContentParser contentParser)
    {
        Http = http;
        MarkdownService = markdownService;
        ContentParser = contentParser;
    }

    #endregion

    #region Blog

    /// <summary>
    /// Get all blog posts sorted by date (newest first).
    /// </summary>
    public async Task<List<BlogPost>> GetBlogPostsAsync()
    {
        if (m_blogPosts != null)
            return m_blogPosts;

        // Prevent race condition with lock
        await m_blogPostsLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (m_blogPosts != null)
                return m_blogPosts;

            var index = await GetContentIndexAsync();
            var posts = new List<BlogPost>();

            foreach (var file in index.Blog)
            {
                try
                {
                    var markdown = await Http.GetStringAsync($"content/blog/{file}");
                    var post = ParseBlogPost(file, markdown);
                    if (post != null)
                        posts.Add(post);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading blog post {file}: {ex.Message}");
                }
            }

            // Deduplicate by slug and sort by date
            m_blogPosts = posts
                .GroupBy(p => p.Slug)
                .Select(g => g.First())
                .OrderByDescending(p => p.PublishDate)
                .ToList();
                
            return m_blogPosts;
        }
        finally
        {
            m_blogPostsLock.Release();
        }
    }

    /// <summary>
    /// Get a specific blog post by slug.
    /// </summary>
    public async Task<BlogPost?> GetBlogPostAsync(string slug)
    {
        var posts = await GetBlogPostsAsync();
        return posts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }
    
    private BlogPost? ParseBlogPost(string filename, string markdown)
    {
        var (frontmatter, html) = MarkdownService.ParseWithFrontmatter<FrontmatterData>(markdown);
        if (frontmatter == null) return null;

        var slug = SlugGenerator.GetSlugFromFilename(filename);
        var content = ExtractContentWithoutFrontmatter(markdown);

        return new BlogPost
        {
            Slug = slug,
            Title = frontmatter.Title ?? slug,
            Description = frontmatter.Description ?? "",
            Summary = frontmatter.Summary ?? "",
            PublishDate = frontmatter.PublishDate,
            Tags = frontmatter.Tags ?? new List<string>(),
            FeaturedImage = frontmatter.FeaturedImage ?? "",
            Author = frontmatter.Author ?? "",
            RawContent = content,
            HtmlContent = html,
            ReadingTimeMinutes = MarkdownService.CalculateReadingTime(content)
        };
    }

    #endregion

    #region Project

    /// <summary>
    /// Get all projects sorted by order (from filename prefix).
    /// </summary>
    public async Task<List<ProjectCard>> GetProjectsAsync()
    {
        if (m_projects != null)
            return m_projects;

        // Prevent race condition with lock
        await m_projectsLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (m_projects != null)
                return m_projects;

            var index = await GetContentIndexAsync();
            var projects = new List<ProjectCard>();

            foreach (var file in index.Projects)
            {
                try
                {
                    var markdown = await Http.GetStringAsync($"content/projects/{file}");
                    var project = ParseProject(file, markdown);
                    if (project != null)
                        projects.Add(project);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading project {file}: {ex.Message}");
                }
            }

            // Deduplicate by slug and sort by order
            m_projects = projects
                .GroupBy(p => p.Slug)
                .Select(g => g.First())
                .OrderBy(p => p.Order)
                .ToList();
                
            return m_projects;
        }
        finally
        {
            m_projectsLock.Release();
        }
    }

    /// <summary>
    /// Get a specific project by slug.
    /// </summary>
    public async Task<ProjectCard?> GetProjectAsync(string slug)
    {
        var projects = await GetProjectsAsync();
        return projects.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }
    
    private ProjectCard? ParseProject(string filename, string markdown)
    {
        var (frontmatter, _) = MarkdownService.ParseWithFrontmatter<FrontmatterData>(markdown);
        if (frontmatter == null) return null;

        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        var content = ExtractContentWithoutFrontmatter(markdown);
        
        // Extract embedded components [[Component ...]] and get processed content
        var (processedContent, components) = ContentParser.Transform(content);
        
        // Set base path for relative URL resolution (for folder-based content)
        var basePath = GetBasePathFromFilename(filename);
        foreach (var component in components)
        {
            component.BasePath = basePath;
        }
        
        // Render processed markdown (with component placeholders) to HTML
        var html = MarkdownService.ToHtml(processedContent);

        return new ProjectCard
        {
            Slug = slug,
            Order = order,
            Title = frontmatter.Title ?? slug,
            Description = frontmatter.Description ?? "",
            Summary = frontmatter.Summary ?? "",
            Tags = frontmatter.Tags ?? new List<string>(),
            Url = frontmatter.Url ?? "",
            MenuTitle = frontmatter.MenuTitle ?? "",
            ShowInMenu = frontmatter.ShowInMenu,
            ShowInHeader = frontmatter.ShowInHeader,
            IsFirstProject = frontmatter.IsFirstProject,
            RawContent = content,
            HtmlContent = html,
            EmbeddedComponents = components
        };
    }


    #endregion

    #region Articles

    /// <summary>
    /// Get all articles sorted by order (from filename prefix).
    /// </summary>
    public async Task<List<ArticleCard>> GetArticlesAsync()
    {
        if (m_articles != null)
            return m_articles;

        await m_articlesLock.WaitAsync();
        try
        {
            if (m_articles != null)
                return m_articles;

            var index = await GetContentIndexAsync();
            var articles = new List<ArticleCard>();

            foreach (var file in index.Articles)
            {
                try
                {
                    var markdown = await Http.GetStringAsync($"content/articles/{file}");
                    var article = ParseArticle(file, markdown, "articles");
                    if (article != null)
                        articles.Add(article);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading article {file}: {ex.Message}");
                }
            }

            m_articles = articles
                .GroupBy(a => a.Slug)
                .Select(g => g.First())
                .OrderBy(a => a.Order)
                .ToList();

            return m_articles;
        }
        finally
        {
            m_articlesLock.Release();
        }
    }

    /// <summary>
    /// Get a specific article by slug.
    /// </summary>
    public async Task<ArticleCard?> GetArticleAsync(string slug)
    {
        var articles = await GetArticlesAsync();
        return articles.FirstOrDefault(a => a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }
    
    private ArticleCard? ParseArticle(string filename, string markdown, string contentFolder)
    {
        var (frontmatter, _) = MarkdownService.ParseWithFrontmatter<FrontmatterData>(markdown);
        if (frontmatter == null) return null;

        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        var content = ExtractContentWithoutFrontmatter(markdown);
        
        // Extract embedded components [[Component ...]] and get processed content
        var (processedContent, components) = ContentParser.Transform(content);
        
        // Set base path for relative URL resolution
        var basePath = GetBasePathForArticle(filename, contentFolder);
        foreach (var component in components)
        {
            component.BasePath = basePath;
        }
        
        // Extract table of contents from headings
        var toc = ExtractHeadings(content);
        
        // Render processed markdown (with component placeholders) to HTML
        var html = MarkdownService.ToHtml(processedContent);

        return new ArticleCard
        {
            Slug = slug,
            Order = order,
            Title = frontmatter.Title ?? slug,
            Description = frontmatter.Description ?? "",
            PublishDate = frontmatter.PublishDate,
            Tags = frontmatter.Tags ?? new List<string>(),
            MenuTitle = frontmatter.MenuTitle ?? "",
            ShowInMenu = frontmatter.ShowInMenu,
            RawContent = content,
            HtmlContent = html,
            TableOfContents = toc,
            EmbeddedComponents = components
        };
    }
    
    /// <summary>
    /// Extract headings (H1-H2) from markdown content for TOC generation.
    /// Skips headings inside code blocks.
    /// Handles duplicate headings by adding numeric suffixes like Markdig.
    /// </summary>
    private static List<TocItem> ExtractHeadings(string markdown)
    {
        var headings = new List<TocItem>();
        var lines = markdown.Split('\n');
        bool inCodeBlock = false;
        var slugCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            
            // Track code block state (``` fenced code blocks)
            if (trimmed.StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }
            
            // Skip lines inside code blocks
            if (inCodeBlock)
                continue;
                
            if (!trimmed.StartsWith('#'))
                continue;
            
            // Must have space after # to be a valid markdown heading
            // This filters out #include, #pragma, etc.
            int level = 0;
            while (level < trimmed.Length && trimmed[level] == '#')
                level++;
            
            // Only include H1, H2 (skip H3 for cleaner TOC)
            if (level < 1 || level > 2)
                continue;
            
            // Must have space after the #'s for valid heading
            if (level >= trimmed.Length || trimmed[level] != ' ')
                continue;
                
            // Extract heading text (remove #'s and space, then trim)
            var text = trimmed[(level + 1)..].Trim();
            if (string.IsNullOrEmpty(text))
                continue;
            
            // Generate slug ID from text using shared utility
            var baseSlug = SlugGenerator.GenerateSlug(text);
            
            // Handle duplicates by adding suffix like Markdig does
            string id;
            if (slugCounts.TryGetValue(baseSlug, out int count))
            {
                slugCounts[baseSlug] = count + 1;
                id = $"{baseSlug}-{count}";
            }
            else
            {
                slugCounts[baseSlug] = 1;
                id = baseSlug;
            }
            
            headings.Add(new TocItem
            {
                Level = level,
                Id = id,
                Text = text
            });
        }
        
        return headings;
    }

    #endregion

    #region Docs

    /// <summary>
    /// Get all documentation pages.
    /// </summary>
    public async Task<List<DocPage>> GetDocsAsync()
    {
        if (m_docs != null)
            return m_docs.Values.OrderBy(d => d.Order).ToList();

        var index = await GetContentIndexAsync();
        m_docs = new Dictionary<string, DocPage>();

        foreach (var file in index.Docs)
        {
            try
            {
                var markdown = await Http.GetStringAsync($"content/docs/{file}");
                var doc = ParseDocPage(file, markdown);
                if (doc != null)
                    m_docs[doc.Slug] = doc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading doc {file}: {ex.Message}");
            }
        }

        // Build navigation links (Previous/Next)
        var sortedDocs = m_docs.Values.OrderBy(d => d.Order).ToList();
        for (int i = 0; i < sortedDocs.Count; i++)
        {
            if (i > 0)
            {
                sortedDocs[i].PreviousPage = new DocNavLink 
                { 
                    Slug = sortedDocs[i - 1].Slug, 
                    Title = sortedDocs[i - 1].Title 
                };
            }
            if (i < sortedDocs.Count - 1)
            {
                sortedDocs[i].NextPage = new DocNavLink 
                { 
                    Slug = sortedDocs[i + 1].Slug, 
                    Title = sortedDocs[i + 1].Title 
                };
            }
        }

        return sortedDocs;
    }

    /// <summary>
    /// Get a specific documentation page by slug.
    /// </summary>
    public async Task<DocPage?> GetDocAsync(string slug)
    {
        var docs = await GetDocsAsync();
        return docs.FirstOrDefault(d => d.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }
    
    private DocPage? ParseDocPage(string filename, string markdown)
    {
        var (frontmatter, html) = MarkdownService.ParseWithFrontmatter<FrontmatterData>(markdown);
        if (frontmatter == null) return null;

        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        var content = ExtractContentWithoutFrontmatter(markdown);
        var toc = MarkdownService.ExtractTableOfContents(content);

        return new DocPage
        {
            Slug = slug,
            Order = order,
            Title = frontmatter.Title ?? slug,
            Description = frontmatter.Description ?? "",
            ParentSlug = frontmatter.Parent ?? "",
            RawContent = content,
            HtmlContent = html,
            TableOfContents = toc
        };
    }

    #endregion

    #region Features

    /// <summary>
    /// Get all features sorted by order (from filename prefix).
    /// </summary>
    public async Task<List<FeatureCard>> GetFeaturesAsync()
    {
        if (m_features != null)
            return m_features;

        await m_featuresLock.WaitAsync();
        try
        {
            if (m_features != null)
                return m_features;

            var index = await GetContentIndexAsync();
            var features = new List<FeatureCard>();

            foreach (var file in index.Features)
            {
                try
                {
                    var markdown = await Http.GetStringAsync($"content/features/{file}");
                    var feature = ParseFeature(file, markdown);
                    if (feature != null)
                        features.Add(feature);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading feature {file}: {ex.Message}");
                }
            }

            m_features = features
                .GroupBy(f => f.Slug)
                .Select(g => g.First())
                .OrderBy(f => f.Order)
                .ToList();

            return m_features;
        }
        finally
        {
            m_featuresLock.Release();
        }
    }

    private FeatureCard? ParseFeature(string filename, string markdown)
    {
        var (frontmatter, _) = MarkdownService.ParseWithFrontmatter<FrontmatterData>(markdown);
        if (frontmatter == null) return null;

        var (order, slug) = SlugGenerator.GetOrderAndSlugFromFilename(filename);
        var content = ExtractContentWithoutFrontmatter(markdown);
        var html = MarkdownService.ToHtml(content);

        return new FeatureCard
        {
            Slug = slug,
            Order = order,
            Title = frontmatter.Title ?? slug,
            Description = frontmatter.Description ?? "",
            Icon = frontmatter.Icon ?? "",
            HtmlContent = html
        };
    }

    #endregion

    #region Tools
    
    /// <summary>
    /// Load the content index which lists all available content files.
    /// </summary>
    private async Task<ContentIndex> GetContentIndexAsync()
    {
        if (m_contentIndex != null)
            return m_contentIndex;

        try
        {
            m_contentIndex = await Http.GetFromJsonAsync<ContentIndex>("content/index.json");
        }
        catch
        {
            m_contentIndex = new ContentIndex();
        }

        return m_contentIndex ?? new ContentIndex();
    }

    /// <summary>
    /// Get base path for article relative URL resolution.
    /// </summary>
    private static string GetBasePathForArticle(string filename, string contentFolder)
    {
        var lastSlash = filename.LastIndexOf('/');
        if (lastSlash > 0)
        {
            var folder = filename[..lastSlash];
            return $"content/{contentFolder}/{folder}";
        }
        return $"content/{contentFolder}";
    }

    private static string ExtractContentWithoutFrontmatter(string markdown)
    {
        // Remove YAML frontmatter
        var match = Regex.Match(markdown, @"^---[\s\S]*?---\s*", RegexOptions.Multiline);
        if (match.Success)
            return markdown.Substring(match.Length).TrimStart();
        return markdown;
    }

    /// <summary>
    /// Get base path for relative URL resolution from filename.
    /// For "01-biography/index.md" returns "content/projects/01-biography"
    /// For "02-cheetah-solver.md" returns "content/projects"
    /// </summary>
    private static string GetBasePathFromFilename(string filename)
    {
        // If filename contains a folder (e.g., "01-biography/index.md")
        var lastSlash = filename.LastIndexOf('/');
        if (lastSlash > 0)
        {
            var folder = filename[..lastSlash];
            return $"content/projects/{folder}";
        }
        return "content/projects";
    }

    #endregion

    #region Properties

    private HttpClient Http { get; }
    
    private MarkdownService MarkdownService { get; }
    
    private ContentParser ContentParser { get; }

    #endregion
}