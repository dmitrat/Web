# OutWit.Web.Framework

Part of [WitDocs](https://witdocs.io) — a Blazor WebAssembly framework for building content-driven static websites with markdown-based content management, SEO optimization, and automatic content generation.

## Features

- Reusable Page Components - HomePage, BlogListPage, ProjectPage, ArticlePage, DocsPage, etc.
- Markdown Content - Write content in markdown with YAML frontmatter
- SEO Optimized - Built-in SeoHead component with Open Graph, Twitter Cards, JSON-LD structured data
- Static Site Generation (SSG) - Pre-rendered HTML pages for search engines
- Open Graph Images - Auto-generated social media preview images
- RSS Feed - Automatic RSS feed generation for blog posts
- Pre-built Search Index - Client-side full-text search with pre-generated index
- **Fast Navigation** - Pre-built navigation index for instant menu rendering
- **Fast List Pages** - Pre-built content metadata for instant list rendering (NEW in v1.3.0)
- Theme Support - Light/dark mode with CSS variables from theme.css
- Responsive Design - Mobile-first CSS framework
- Multiple Hosting Providers - Cloudflare Pages, Netlify, Vercel, GitHub Pages

## Installation

### From NuGet

```bash
dotnet add package OutWit.Web.Framework
```

### From Source

```xml
<ProjectReference Include="path/to/OutWit.Web.Framework.csproj" />
```

## Quick Start

### 1. Project Structure

```
your-site/
    Pages/               # Thin page wrappers
    wwwroot/
        content/         # Markdown content
            blog/
            projects/
            articles/
            docs/
        css/
            theme.css    # Your color scheme
            site.css     # Site-specific styles
        images/
        site.config.json
    Program.cs
    YourSite.csproj
```

### 2. Configure Your Site

Create `wwwroot/site.config.json`:

```json
{
  "siteName": "My Site",
  "baseUrl": "https://example.com",
  "logo": "/images/logo.svg",
  "defaultTheme": "dark",
  "navigation": [
    { "title": "Home", "href": "/" },
    { "title": "Blog", "href": "/blog" },
    { "title": "Contact", "href": "/contact" }
  ],
  "footer": {
    "copyright": "Your Name",
    "socialLinks": [
      { "platform": "github", "url": "https://github.com/you" }
    ]
  },
  "seo": {
    "defaultImage": "/images/social-card.png",
    "twitterHandle": "@yourhandle"
  }
}
```

### 3. Create Theme Colors

Create `wwwroot/css/theme.css`:

```css
:root {
    --color-background: #ffffff;
    --color-text-primary: #333333;
    --color-accent: #007CF0;
    /* ... other variables */
}

[data-theme="dark"] {
    --color-background: #1f1f1f;
    --color-text-primary: #d1d1d1;
    --color-accent: #00a3ff;
}
```

### 4. Create Pages

```razor
@* Pages/Index.razor *@
@page "/"
@using OutWit.Web.Framework.Components.Pages

<HomePage />
```

```razor
@* Pages/Blog.razor *@
@page "/blog"
@using OutWit.Web.Framework.Components.Pages

<BlogListPage />
```

```razor
@* Pages/BlogPost.razor *@
@page "/blog/{Slug}"
@using OutWit.Web.Framework.Components.Pages

<BlogPostPage Slug="@Slug" />

@code {
    [Parameter] public string Slug { get; set; } = "";
}
```

## Content Structure

### Markdown with Frontmatter

```yaml
---
title: 'My Blog Post'
description: 'Short description for SEO'
summary: |
  Longer summary with **markdown** support
tags: [blazor, dotnet, web]
publishDate: 2024-01-15
menuTitle: 'Short Title'    # Optional: short title for navigation menus
showInMenu: true            # Show in dropdown menus (default: true)
showInHeader: false         # Show as top-level nav item (projects only)
---

# Content starts here

Your markdown content...
```

### Content Folders

```
wwwroot/content/
    index.json           # Auto-generated manifest
    blog/
        2024-01-15-post-title.md
    projects/
        01-project-name/
            index.md
            image.png
    articles/
        my-article.md
    docs/
        getting-started.md
```

## Page Components

| Component | Route | Description |
|-----------|-------|-------------|
| HomePage | / | Home page with hero and projects |
| BlogListPage | /blog | Blog listing with search |
| BlogPostPage | /blog/{slug} | Individual blog post |
| ProjectPage | /project/{slug} | Project detail page |
| ArticlePage | /article/{slug} | Article with TOC |
| DocsPage | /docs/{slug} | Documentation page |
| SearchPage | /search | Search results |
| ContactPage | /contact | Contact form |
| NotFoundPage | * | 404 page |

## Build Configuration

### MSBuild Properties

```xml
<PropertyGroup>
  <!-- Force content generation (default: Release only) -->
  <OutWitGenerateContent>true</OutWitGenerateContent>
  
  <!-- Enable generation in Debug mode (default: false) -->
  <OutWitGenerateInDebug>true</OutWitGenerateInDebug>
  
  <!-- Disable specific generators -->
  <OutWitGenerateStaticPages>true</OutWitGenerateStaticPages>
  <OutWitGenerateSearchIndex>true</OutWitGenerateSearchIndex>
  <OutWitGenerateRssFeed>true</OutWitGenerateRssFeed>
  <OutWitGenerateOgImages>false</OutWitGenerateOgImages>
  
  <!-- Hosting provider: cloudflare, netlify, vercel, github, none -->
  <OutWitHostingProvider>cloudflare</OutWitHostingProvider>
</PropertyGroup>
```

### Development Mode

For faster development experience, enable content generation in Debug mode:

```xml
<PropertyGroup>
  <OutWitGenerateInDebug>true</OutWitGenerateInDebug>
</PropertyGroup>
```

This generates navigation and metadata indices during Debug builds, providing:
- Instant menu rendering (no markdown parsing)
- Fast list pages (BlogListPage, HomePage)
- Static HTML pages are NOT generated in Debug mode by default

Without this option, the framework falls back to loading content directly (slower but works).

### Automatic Generation

On Release build, the framework automatically runs the [OutWit.Web.Generator](../OutWit.Web.Generator/) tool to generate:

- `content/index.json` - Content manifest
- `navigation-index.json` - Pre-built navigation menu data (v1.2.0+)
- `content-metadata.json` - Pre-built content metadata for lists (NEW in v1.3.0)
- `sitemap.xml` - SEO sitemap
- `robots.txt` - Crawler rules
- `search-index.json` - Pre-built search index
- `feed.xml` - RSS feed for blog
- `*/index.html` - Static HTML pages for SEO
- `_headers`, `_redirects` - Hosting provider config

**Prerequisite:** Install the Generator tool globally:

```bash
dotnet tool install -g OutWit.Web.Generator
```

Then simply build in Release mode:

```bash
dotnet build -c Release
```

### Manual Generation

```bash
outwit-generate \
  --content-path ./wwwroot/content \
  --output-path ./wwwroot \
  --site-url https://example.com \
  --site-name "My Site"
```

For OG images (requires Playwright):

```bash
npx playwright install chromium
outwit-generate --content-path ./wwwroot/content --output-path ./wwwroot
```

## Performance Optimization

### Navigation Index (v1.2.0+)

The framework generates a `navigation-index.json` file containing pre-built menu data. This eliminates the need to parse all markdown files when loading the header navigation, resulting in significantly faster page loads for sites with many content items.

### Content Metadata Index (v1.3.0+)

The framework now generates a `content-metadata.json` file containing lightweight metadata for all content items. This allows list pages (HomePage, BlogListPage) to render instantly without parsing individual markdown files.

**Before v1.3.0:** List pages loaded and parsed ALL markdown files to display titles, descriptions, and tags

**After v1.3.0:** List pages load a single small JSON file with pre-extracted metadata

The content metadata index includes:
- Blog posts: slug, title, description, summary, tags, publishDate, author, readingTime, featuredImage
- Projects: slug, title, description, summary, order, tags, url
- Articles: slug, title, description, order, tags, publishDate
- Docs: slug, title, description, order, parentSlug
- Features: slug, title, description, order, icon, iconSvg
- Dynamic sections support

During development (when the metadata index is not available), the framework falls back to loading content directly.

## Open Graph Images

The framework can generate social media preview images (1200x630 PNG) for each content page.

Colors are automatically read from your `theme.css`:
- `--color-accent` - Accent color for highlights
- `--color-background` - Background color

To enable in build:

```xml
<OutWitGenerateOgImages>true</OutWitGenerateOgImages>
```

Requires Playwright:

```bash
npm install -D playwright
npx playwright install chromium
```

## Services

| Service | Description |
|---------|-------------|
| ContentService | Load and parse markdown content |
| ConfigService | Load site configuration |
| NavigationService | Load pre-built navigation index (v1.2.0+) |
| ContentMetadataService | Load pre-built content metadata (NEW in v1.3.0) |
| SearchService | Full-text search with pre-built index |
| MarkdownService | Parse markdown to HTML |
| ThemeService | Theme switching (light/dark) |

## Deployment

### Cloudflare Pages

```yaml
- name: Build
  run: dotnet publish -c Release

- name: Deploy
  uses: cloudflare/pages-action@v1
  with:
    directory: publish/wwwroot
```

### GitHub Actions Example

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Build
        run: dotnet publish -c Release -o publish
      
      - name: Deploy to Cloudflare Pages
        uses: cloudflare/pages-action@v1
        with:
          apiToken: ${{ secrets.CLOUDFLARE_API_TOKEN }}
          accountId: ${{ secrets.CLOUDFLARE_ACCOUNT_ID }}
          projectName: your-site
          directory: publish/wwwroot
```

## License

Licensed under the Apache License, Version 2.0. See `LICENSE`.

## Attribution (optional)

If you use WitDocs in a product, a mention is appreciated (but not required), for example:
"Built with [WitDocs](https://witdocs.io)".

## Trademark / Project name

"WitDocs" and "OutWit" are used to identify the official project by Dmitry Ratner.

You may:
- refer to the project name in a factual way (e.g., "built with WitDocs");
- use the name to indicate compatibility (e.g., "WitDocs-compatible").

You may not:
- use "WitDocs" as the name of a fork or a derived product in a way that implies it is the official project;
- use the WitDocs logo to promote forks or derived products without permission.
