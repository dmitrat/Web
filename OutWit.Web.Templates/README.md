# OutWit.Web.Templates

Part of [WitDocs](https://witdocs.io) — project templates for creating static websites using WitDocs framework.

## Installation

```bash
dotnet new install OutWit.Web.Templates
```

## Usage

### Create a new site

```bash
# Basic usage
dotnet new outwit-web -n MySite

# With all options
dotnet new outwit-web -n MySite \
  --site-name "My Awesome Site" \
  --base-url "https://mysite.com" \
  --author-name "John Doe" \
  --accent-color "#FF6B6B" \
  --github-url "https://github.com/johndoe" \
  --twitter-handle "@johndoe" \
  --hosting-provider cloudflare \
  --enable-debug-generation
```

### Available Options

| Option | Description | Default |
|--------|-------------|---------|
| `--site-name` | Display name of your site | My Site |
| `--base-url` | Base URL (e.g., https://example.com) | https://example.com |
| `--author-name` | Author name for copyright | Your Name |
| `--accent-color` | Primary accent color (hex) | #007CF0 |
| `--github-url` | GitHub profile/repo URL | (empty) |
| `--twitter-handle` | Twitter handle | (empty) |
| `--hosting-provider` | Target hosting (cloudflare/netlify/vercel/github/none) | cloudflare |
| `--include-docs-section` | Include docs pages | false |
| `--include-blog-section` | Include blog pages | true |
| `--include-projects-section` | Include projects pages | true |
| `--enable-debug-generation` | Enable content generation in Debug mode (v1.3.0+) | false |

### Run your site

```bash
cd MySite
dotnet run
```

### Build for production

```bash
dotnet publish -c Release
```

## Project Structure

```
MySite/
  Pages/
    Index.razor
    Blog.razor
    BlogPost.razor
    Project.razor
    Contact.razor
    Search.razor
  wwwroot/
    content/
      blog/
      projects/
    css/
      theme.css
      site.css
    images/
    site.config.json
    index.html
  Program.cs
  MySite.csproj
```

## Customization

1. **Colors**: Edit `wwwroot/css/theme.css`
2. **Navigation**: Edit `wwwroot/site.config.json`
3. **Content**: Add markdown files to `wwwroot/content/` folders
4. **Logo**: Replace `wwwroot/images/logo.svg`

## What's New in v1.3.0

- **Skeleton Loading**: Built-in skeleton components for better loading UX
- **Content Metadata Index**: Faster list page rendering
- **Debug Mode Generation**: Option to generate content indices in Debug builds
- **Direct Content Loading**: Load individual content items without parsing all files

## Uninstall

```bash
dotnet new uninstall OutWit.Web.Templates
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
