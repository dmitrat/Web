# OutWit.Web.Generator

Part of [WitDocs](https://witdocs.io) — a .NET CLI tool for generating static content, OG images, sitemap, search index, and RSS feeds for WitDocs sites.

## Installation

```bash
dotnet tool install -g OutWit.Web.Generator
```

## Usage

### Automatic Generation (Recommended)

When using **OutWit.Web.Framework**, content generation runs automatically on **Release builds**:

1. Install the Generator tool globally
2. In Visual Studio, switch to **Release** configuration
3. Build your project (Ctrl+Shift+B)
4. Generation runs automatically after build!

### Manual CLI Usage

```bash
outwit-generate --content-path ./wwwroot/content --output-path ./wwwroot
```

## CLI Options

| Option | Description | Default |
|--------|-------------|---------|
| `--content-path` | Path to content folder | Required |
| `--output-path` | Output directory | `site/wwwroot` |
| `--site-url` | Base URL for sitemap/RSS | `https://example.com` |
| `--site-name` | Site name for RSS feed | `My Site` |
| `--hosting` | Hosting provider (cloudflare/netlify/vercel/github) | `cloudflare` |
| `--no-sitemap` | Skip sitemap generation | false |
| `--no-search` | Skip search index generation | false |
| `--no-rss` | Skip RSS feed generation | false |
| `--no-static` | Skip static HTML generation | false |
| `--no-og` | Skip OG image generation | false |
| `--force-og` | Force regenerate OG images | false |
| `--search-content-max-length` | Max content length for search index | 10000 |

## MSBuild Properties

When using the Framework, configure generation via MSBuild properties in your `.csproj`:

```xml
<PropertyGroup>
  <!-- Site URL (required for sitemap/RSS) -->
  <OutWitSiteUrl>https://example.com</OutWitSiteUrl>
  
  <!-- Site name (for RSS feed) -->
  <OutWitSiteName>My Site</OutWitSiteName>
  
  <!-- Hosting provider: cloudflare, netlify, vercel, github, none -->
  <OutWitHostingProvider>cloudflare</OutWitHostingProvider>
  
  <!-- Enable/disable specific features -->
  <OutWitGenerateStaticPages>true</OutWitGenerateStaticPages>
  <OutWitGenerateSearchIndex>true</OutWitGenerateSearchIndex>
  <OutWitGenerateRssFeed>true</OutWitGenerateRssFeed>
  <OutWitGenerateOgImages>false</OutWitGenerateOgImages>
  
  <!-- Force generation even in Debug mode -->
  <OutWitGenerateContent>true</OutWitGenerateContent>
  
  <!-- Enable generation in Debug mode (v1.2.0+) -->
  <OutWitGenerateInDebug>true</OutWitGenerateInDebug>
</PropertyGroup>
```

## Generated Files

| File | Description |
|------|-------------|
| `content/index.json` | Content manifest listing all files |
| `navigation-index.json` | Pre-built navigation menu data |
| `content-metadata.json` | Pre-built content metadata for fast list rendering |
| `sitemap.xml` | SEO sitemap |
| `robots.txt` | Crawler rules |
| `search-index.json` | Pre-built search index |
| `feed.xml` | RSS feed for blog |
| `*/index.html` | Static HTML pages |

### Hosting Provider Files

| Provider | Files Generated |
|----------|-----------------|
| **Cloudflare** | `_headers`, `_routes.json` |
| **Netlify** | `_headers`, `_redirects` |
| **Vercel** | `vercel.json` |
| **GitHub Pages** | `.nojekyll`, `404.html` |

## Features

### Content Index
Generates `index.json` listing all content files by category (blog, projects, docs, articles, features) and dynamic sections.

### Navigation Index (v1.1.0+)
Generates `navigation-index.json` for instant menu rendering without parsing markdown files.

### Content Metadata Index (v1.2.0+)
Generates `content-metadata.json` for fast list page rendering (BlogListPage, HomePage) without parsing all markdown files.

### Sitemap
Creates `sitemap.xml` and `robots.txt` with proper lastmod dates.

### Search Index
Generates `search-index.json` for client-side search functionality.

### RSS Feed
Creates `feed.xml` for blog posts with proper formatting.

### Static HTML
Pre-renders HTML pages for SEO and faster initial load.

### OG Images
Generates Open Graph images for social sharing using Playwright.

```bash
# Install Playwright before using OG images
npx playwright install chromium
```

## Dynamic Content Sections

Define custom content sections in `site.config.json`:

```json
{
  "contentSections": [
    { "folder": "solutions", "route": "solutions", "menuTitle": "Solutions" }
  ]
}
```

## GitHub Actions

Example workflow step:

```yaml
- name: Build and Run Generator
  run: |
    dotnet build OutWit.Web.Generator -c Release
    dotnet run --project OutWit.Web.Generator -c Release -- \
      --content-path publish/wwwroot/content \
      --output-path publish/wwwroot \
      --site-url https://example.com \
      --site-name "My Site"
```

## What's New in v1.3.1

- **Cloudflare `_routes.json`**: Added explicit SPA routing configuration for Cloudflare Pages
- **Fixed markdown rendering**: Description and Summary fields now correctly render markdown to HTML

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
