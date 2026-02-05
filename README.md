# WitDocs

A Blazor WebAssembly platform for building content-driven static websites with markdown content, SEO optimization, and automatic content generation.

**Website:** [https://witdocs.io](https://witdocs.io)

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| [OutWit.Web.Framework](OutWit.Web.Framework/) | Core framework with components, services, and build tools | [![NuGet](https://img.shields.io/nuget/v/OutWit.Web.Framework.svg)](https://www.nuget.org/packages/OutWit.Web.Framework/) |
| [OutWit.Web.Generator](OutWit.Web.Generator/) | CLI tool for content generation (sitemap, RSS, search, OG images) | [![NuGet](https://img.shields.io/nuget/v/OutWit.Web.Generator.svg)](https://www.nuget.org/packages/OutWit.Web.Generator/) |
| [OutWit.Web.Templates](OutWit.Web.Templates/) | dotnet new templates for creating new sites | [![NuGet](https://img.shields.io/nuget/v/OutWit.Web.Templates.svg)](https://www.nuget.org/packages/OutWit.Web.Templates/) |

## Quick Start

### Option 1: Use the Template (Recommended)

```bash
# Install the template
dotnet new install OutWit.Web.Templates

# Create a new site
dotnet new outwit-web -n MySite -s "My Awesome Site" -b "https://mysite.com"

# Run the site
cd MySite
dotnet run
```

### Option 2: Add to Existing Project

```bash
dotnet add package OutWit.Web.Framework
```

## Features

- **Reusable Page Components** - HomePage, BlogListPage, ProjectPage, ArticlePage, DocsPage, etc.
- **Markdown Content** - Write content in markdown with YAML frontmatter
- **SEO Optimized** - Open Graph, Twitter Cards, JSON-LD structured data, sitemap, robots.txt
- **Static Site Generation** - Pre-rendered HTML pages for search engines
- **Open Graph Images** - Auto-generated social media preview images
- **RSS Feed** - Automatic RSS feed generation for blog posts
- **Full-Text Search** - Client-side search with pre-generated index
- **Theme Support** - Light/dark mode with CSS variables
- **Multiple Hosting Providers** - Cloudflare Pages, Netlify, Vercel, GitHub Pages

## Project Structure

```
your-site/
    Pages/                   # Thin page wrappers
        Index.razor
        Blog.razor
        BlogPost.razor
        Project.razor
        Contact.razor
        Search.razor
    wwwroot/
        content/             # Markdown content
            blog/
            projects/
            articles/
            docs/
        css/
            theme.css        # Your color scheme
            site.css         # Site-specific styles
        images/
            logo-light.svg   # Logo for light theme
            logo-dark.svg    # Logo for dark theme
        site.config.json     # Site configuration
    Program.cs
    YourSite.csproj
```

## Configuration

### site.config.json

```json
{
  "siteName": "My Site",
  "baseUrl": "https://example.com",
  "logoLight": "/images/logo-light.svg",
  "logoDark": "/images/logo-dark.svg",
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

### theme.css

```css
:root {
    --color-background: #ffffff;
    --color-text-primary: #333333;
    --color-accent: #007CF0;
}

[data-theme="dark"] {
    --color-background: #1f1f1f;
    --color-text-primary: #d1d1d1;
    --color-accent: #00a3ff;
}
```

## Template Options

```bash
dotnet new outwit-web --help
```

| Option | Description | Default |
|--------|-------------|---------|
| `-s, --siteName` | Display name of your site | My Site |
| `-b, --baseUrl` | Base URL (https://...) | https://example.com |
| `-au, --authorName` | Author name for copyright | Your Name |
| `-ac, --accentColor` | Primary accent color (hex) | #007CF0 |
| `-g, --githubUrl` | GitHub profile/repo URL | (empty) |
| `-tw, --twitterHandle` | Twitter handle | (empty) |
| `-ho, --hostingProvider` | cloudflare/netlify/vercel/github/none | cloudflare |
| `--includeDocsSection` | Include documentation pages | false |
| `--includeBlogSection` | Include blog pages | true |
| `--includeProjectsSection` | Include projects pages | true |

## Build and Deploy

### Development

```bash
dotnet run
```

### Production Build

**Prerequisite:** Install the Generator tool:

```bash
dotnet tool install -g OutWit.Web.Generator
```

Then build in Release mode:

```bash
dotnet build -c Release
```

The framework automatically runs the Generator to create:
- `content/index.json` - Content manifest
- `sitemap.xml` - SEO sitemap
- `robots.txt` - Crawler rules
- `search-index.json` - Pre-built search index
- `feed.xml` - RSS feed for blog
- Static HTML pages for SEO
- Hosting provider config files

### Deploy to Cloudflare Pages

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Build
        run: dotnet publish -c Release -o publish
      
      - name: Deploy
        uses: cloudflare/pages-action@v1
        with:
          apiToken: ${{ secrets.CLOUDFLARE_API_TOKEN }}
          accountId: ${{ secrets.CLOUDFLARE_ACCOUNT_ID }}
          projectName: your-site
          directory: publish/wwwroot
```

## Documentation

- [OutWit.Web.Framework README](OutWit.Web.Framework/README.md) - Detailed framework documentation
- [OutWit.Web.Generator README](OutWit.Web.Generator/README.md) - Generator CLI documentation
- [OutWit.Web.Templates README](OutWit.Web.Templates/README.md) - Template usage guide

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
