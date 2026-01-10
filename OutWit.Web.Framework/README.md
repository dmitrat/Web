# OutWit.Web.Framework

A Blazor WebAssembly framework for building content-driven static websites with markdown-based content management, SEO optimization, and automatic content generation.

## Features

- Reusable Page Components - HomePage, BlogListPage, ProjectPage, ArticlePage, DocsPage, etc.
- Markdown Content - Write content in markdown with YAML frontmatter
- SEO Optimized - Built-in SeoHead component with Open Graph, Twitter Cards, JSON-LD structured data
- Static Site Generation (SSG) - Pre-rendered HTML pages for search engines
- Open Graph Images - Auto-generated social media preview images
- RSS Feed - Automatic RSS feed generation for blog posts
- Pre-built Search Index - Client-side full-text search with pre-generated index
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
  
  <!-- Disable specific generators -->
  <OutWitGenerateStaticPages>true</OutWitGenerateStaticPages>
  <OutWitGenerateSearchIndex>true</OutWitGenerateSearchIndex>
  <OutWitGenerateRssFeed>true</OutWitGenerateRssFeed>
  <OutWitGenerateOgImages>false</OutWitGenerateOgImages>
  
  <!-- Hosting provider: cloudflare, netlify, vercel, github, none -->
  <OutWitHostingProvider>cloudflare</OutWitHostingProvider>
</PropertyGroup>
```

### Automatic Generation

On Release build, the framework automatically generates:

- `content/index.json` - Content manifest
- `sitemap.xml` - SEO sitemap
- `robots.txt` - Crawler rules
- `search-index.json` - Pre-built search index
- `feed.xml` - RSS feed for blog
- `*/index.html` - Static HTML pages for SEO
- `_headers`, `_redirects` - Hosting provider config

### Manual Generation

```powershell
# Generate all content
./scripts/generate-content.ps1 -SitePath "path/to/your-site"

# Generate OG images (requires Playwright)
npm install -D playwright
npx playwright install chromium
./scripts/generate-og-images.ps1 -SitePath "path/to/your-site"
```

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

This software is licensed under the **Non-Commercial License (NCL)**.

- Free for personal, educational, and research purposes
- Commercial use requires a separate license agreement
- Contact licensing@ratner.io for commercial licensing inquiries

See the full [LICENSE](LICENSE) file for details.
