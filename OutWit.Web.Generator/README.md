# OutWit.Web.Generator

A .NET CLI tool for generating static content, OG images, sitemap, search index, and RSS feeds for OutWit.Web sites.

## Installation

```bash
dotnet tool install -g OutWit.Web.Generator
```

## Usage

```bash
outwit-generate --content-path ./site/wwwroot/content --output-path ./site/wwwroot
```

### Options

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

## Features

### Content Index
Generates `index.json` listing all content files by category (blog, projects, docs, articles, features).

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

> **Note:** Run `playwright install chromium` before generating OG images.

## Dynamic Content Sections

Define custom content sections in `site.config.json`:

```json
{
  "contentSections": [
    { "folder": "solutions", "route": "solutions", "menuTitle": "Solutions" }
  ]
}
```

## License

This software is licensed under the **Non-Commercial License (NCL)**.

- Free for personal, educational, and research purposes
- Commercial use requires a separate license agreement
- Contact licensing@ratner.io for commercial licensing inquiries

See the full [LICENSE](LICENSE) file for details.
