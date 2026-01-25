# OutWit.Web.Templates

Project templates for creating static websites using OutWit.Web.Framework.

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
  --hosting-provider cloudflare
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

## Uninstall

```bash
dotnet new uninstall OutWit.Web.Templates
```

## License

Licensed under the Apache License, Version 2.0. See `LICENSE`.

## Attribution (optional)

If you use OutWit.Web.Templates in a product, a mention is appreciated (but not required), for example:
"Powered by OutWit.Web.Templates (https://ratner.io/)".

## Trademark / Project name

"OutWit" and the OutWit logo are used to identify the official project by Dmitry Ratner.

You may:
- refer to the project name in a factual way (e.g., "built with OutWit.Web.Templates");
- use the name to indicate compatibility (e.g., "OutWit.Web.Templates-compatible").

You may not:
- use "OutWit.Web.Templates" as the name of a fork or a derived product in a way that implies it is the official project;
- use the OutWit.Web.Templates logo to promote forks or derived products without permission.
