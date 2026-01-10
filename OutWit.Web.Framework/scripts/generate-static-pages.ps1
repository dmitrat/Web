<#
.SYNOPSIS
    Generate static HTML pages for SEO (Static Site Generation).
    
.DESCRIPTION
    Generates pre-rendered HTML pages from markdown content for SEO.
    The generated HTML contains full content visible to search engines,
    while Blazor WASM hydrates the page after loading.
    
.PARAMETER SitePath
    Path to the site folder (e.g., "sites/ratner.io")
    
.PARAMETER OutputPath
    Path to output folder (defaults to SitePath/wwwroot)

.PARAMETER HostingProvider
    Target hosting provider. Generates provider-specific config files.
    Options: "cloudflare", "netlify", "vercel", "github", "none"
    Default: "cloudflare"

.EXAMPLE
    .\generate-static-pages.ps1 -SitePath "sites/ratner.io"

.EXAMPLE
    .\generate-static-pages.ps1 -SitePath "sites/ratner.io" -HostingProvider "netlify"

.EXAMPLE
    .\generate-static-pages.ps1 -SitePath "sites/ratner.io" -HostingProvider "none"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SitePath,
    [string]$OutputPath,
    [ValidateSet("cloudflare", "netlify", "vercel", "github", "none")]
    [string]$HostingProvider = "cloudflare"
)

$ErrorActionPreference = "Stop"

# Normalize paths
$SitePath = $SitePath.TrimEnd("/", "\")
if (-not $OutputPath) {
    $OutputPath = Join-Path $SitePath "wwwroot"
}

$ContentPath = Join-Path $OutputPath "content"
$ConfigPath = Join-Path $OutputPath "site.config.json"
$TemplateHtmlPath = Join-Path $OutputPath "index.html"

# Validate paths
if (-not (Test-Path $ConfigPath)) {
    throw "site.config.json not found at $ConfigPath"
}
if (-not (Test-Path $TemplateHtmlPath)) {
    throw "index.html template not found at $TemplateHtmlPath"
}

# Load configuration
$config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$siteUrl = $config.baseUrl
$siteName = $config.siteName

# Get logo URL for og:logo (use dark logo for better visibility on light backgrounds)
$logoUrl = $null
if ($config.logoDark) {
    $logo = $config.logoDark
    if ($logo.StartsWith("http")) {
        $logoUrl = $logo
    } else {
        $logoUrl = "$siteUrl$logo"
    }
}

Write-Host "Generating static HTML pages..." -ForegroundColor Cyan
Write-Host "  Site: $siteName ($siteUrl)"
Write-Host "  Content: $ContentPath"
Write-Host "  Output: $OutputPath"
Write-Host "  Hosting: $HostingProvider"

# Load template HTML
$templateHtml = Get-Content $TemplateHtmlPath -Raw

# Helper function for null coalescing
function Coalesce {
    param($a, $b)
    if ($a) { return $a } else { return $b }
}

# Simple YAML frontmatter parser
function Parse-Frontmatter {
    param([string]$Content)
    
    $result = @{
        title       = ""
        description = ""
        summary     = ""
        publishDate = $null
        tags        = @()
        content     = $Content
    }
    
    if ($Content -match "^---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n([\s\S]*)$") {
        $yaml = $Matches[1]
        $result.content = $Matches[2]
        
        # Parse YAML fields
        if ($yaml -match "title:\s*['""]?([^'""'\r\n]+)['""]?") {
            $result.title = $Matches[1].Trim()
        }
        if ($yaml -match "description:\s*['""]?([^'""'\r\n]+)['""]?") {
            $result.description = $Matches[1].Trim()
        }
        if ($yaml -match "summary:\s*\|?\s*\r?\n([\s\S]*?)(?=\r?\n[a-zA-Z]|\r?\n---|\z)") {
            $result.summary = $Matches[1].Trim() -replace "^\s+", "" -replace "\r?\n\s+", " "
        }
        elseif ($yaml -match "summary:\s*['""]?([^'""'\r\n]+)['""]?") {
            $result.summary = $Matches[1].Trim()
        }
        if ($yaml -match "publishDate:\s*(\d{4}-\d{2}-\d{2})") {
            $result.publishDate = [DateTime]::Parse($Matches[1])
        }
        if ($yaml -match "tags:\s*\[([^\]]+)\]") {
            $result.tags = $Matches[1] -split "," | ForEach-Object { $_.Trim().Trim("'", '"') }
        }
    }
    
    return $result
}

# Convert markdown to HTML (basic conversion for SSG)
function Convert-MarkdownToHtml {
    param([string]$Markdown)
    
    $html = $Markdown
    
    # Code blocks (must be first to protect content)
    $html = [regex]::Replace($html, '```(\w*)\r?\n([\s\S]*?)```', {
        param($m)
        $lang = $m.Groups[1].Value
        $code = [System.Web.HttpUtility]::HtmlEncode($m.Groups[2].Value.TrimEnd())
        "<pre><code class=`"language-$lang`">$code</code></pre>"
    })
    
    # Inline code
    $html = [regex]::Replace($html, '`([^`]+)`', '<code>$1</code>')
    
    # Headers
    $html = [regex]::Replace($html, '^#{6}\s+(.+)$', '<h6>$1</h6>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $html = [regex]::Replace($html, '^#{5}\s+(.+)$', '<h5>$1</h5>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $html = [regex]::Replace($html, '^#{4}\s+(.+)$', '<h4>$1</h4>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $html = [regex]::Replace($html, '^#{3}\s+(.+)$', '<h3>$1</h3>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $html = [regex]::Replace($html, '^#{2}\s+(.+)$', '<h2>$1</h2>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $html = [regex]::Replace($html, '^#\s+(.+)$', '<h1>$1</h1>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Bold and italic
    $html = [regex]::Replace($html, '\*\*\*(.+?)\*\*\*', '<strong><em>$1</em></strong>')
    $html = [regex]::Replace($html, '\*\*(.+?)\*\*', '<strong>$1</strong>')
    $html = [regex]::Replace($html, '\*(.+?)\*', '<em>$1</em>')
    
    # Links
    $html = [regex]::Replace($html, '\[([^\]]+)\]\(([^)]+)\)', '<a href="$2">$1</a>')
    
    # Images
    $html = [regex]::Replace($html, '!\[([^\]]*)\]\(([^)]+)\)', '<img src="$2" alt="$1" loading="lazy" />')
    
    # Blockquotes
    $html = [regex]::Replace($html, '^>\s+(.+)$', '<blockquote>$1</blockquote>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Horizontal rules
    $html = [regex]::Replace($html, '^---+$', '<hr />', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Unordered lists (simple)
    $html = [regex]::Replace($html, '^[-*]\s+(.+)$', '<li>$1</li>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Ordered lists (simple)
    $html = [regex]::Replace($html, '^\d+\.\s+(.+)$', '<li>$1</li>', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Wrap consecutive <li> in <ul>
    $html = [regex]::Replace($html, '((?:<li>.*?</li>\s*)+)', '<ul>$1</ul>')
    
    # Paragraphs (wrap text blocks)
    $lines = $html -split '\r?\n'
    $result = @()
    $inParagraph = $false
    $paragraphContent = @()
    
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            if ($inParagraph -and $paragraphContent.Count -gt 0) {
                $result += "<p>" + ($paragraphContent -join " ") + "</p>"
                $paragraphContent = @()
            }
            $inParagraph = $false
        }
        elseif ($trimmed -match '^<(h[1-6]|pre|ul|ol|blockquote|hr|div|table)') {
            if ($inParagraph -and $paragraphContent.Count -gt 0) {
                $result += "<p>" + ($paragraphContent -join " ") + "</p>"
                $paragraphContent = @()
            }
            $inParagraph = $false
            $result += $trimmed
        }
        elseif ($trimmed -match '</(pre|ul|ol|blockquote|div|table)>$') {
            $result += $trimmed
            $inParagraph = $false
        }
        else {
            $inParagraph = $true
            $paragraphContent += $trimmed
        }
    }
    
    if ($paragraphContent.Count -gt 0) {
        $result += "<p>" + ($paragraphContent -join " ") + "</p>"
    }
    
    return $result -join "`n"
}

# Generate slug from filename
function Get-SlugFromPath {
    param([string]$Path)
    
    $name = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    
    # Handle folder-based content (01-biography/index.md -> biography)
    if ($name -eq "index") {
        $parent = Split-Path -Path $Path -Parent
        if ($parent) {
            $name = Split-Path -Path $parent -Leaf
        }
    }
    
    # Remove date prefix for blog posts (2024-11-20-title -> title)
    if ($name -match "^\d{4}-\d{2}-\d{2}-") {
        $name = $name -replace "^\d{4}-\d{2}-\d{2}-", ""
    }
    # Remove leading order numbers (e.g., "01-biography" -> "biography")
    elseif ($name -match "^\d+-") {
        $name = $name -replace "^\d+-", ""
    }
    
    return $name
}

# Generate static HTML page
function Generate-StaticPage {
    param(
        [string]$Title,
        [string]$Description,
        [string]$HtmlContent,
        [string]$CanonicalUrl,
        [string]$OgType = "website",
        [string]$OgImage = "",
        $PublishDate = $null,
        [string[]]$Tags = @()
    )
    
    $html = $templateHtml
    
    # Build page title
    $pageTitle = if ($Title) { "$Title - $siteName" } else { $siteName }
    
    # Build meta description
    $metaDescription = if ($Description) { $Description } else { "Welcome to $siteName" }
    $metaDescription = $metaDescription -replace '"', '&quot;'
    
    # Build OG image URL (auto-detect if not provided)
    $ogImageUrl = $OgImage
    if (-not $ogImageUrl -and $CanonicalUrl) {
        # Auto-generate OG image path based on URL pattern
        $urlPath = $CanonicalUrl -replace [regex]::Escape($siteUrl), ""
        $urlPath = $urlPath.TrimStart("/").TrimEnd("/")
        
        if (-not $urlPath -or $urlPath -eq "") {
            $ogImageUrl = "$siteUrl/og-images/default.png"
        }
        else {
            $segments = $urlPath -split "/"
            if ($segments.Count -ge 2) {
                $contentType = $segments[0]
                $slug = $segments[1]
                $ogImageUrl = "$siteUrl/og-images/$contentType-$slug.png"
            }
        }
    }
    
    # Build tags HTML
    $tagsHtml = ""
    if ($Tags.Count -gt 0) {
        $tagsHtml = '<div class="tags">' + ($Tags | ForEach-Object { "<span class=`"tag`">$_</span>" }) -join "" + '</div>'
    }
    
    # Build date HTML
    $dateHtml = ""
    if ($PublishDate) {
        $dateFormatted = $PublishDate.ToString("MMMM d, yyyy")
        $dateIso = $PublishDate.ToString("yyyy-MM-dd")
        $dateHtml = "<time datetime=`"$dateIso`">$dateFormatted</time>"
    }
    
    # Build article header
    $headerHtml = ""
    if ($Title) {
        $headerHtml = @"
<header class="page-header">
    <h1 class="page-header__title">$Title</h1>
    $(if ($dateHtml) { "<div class=`"page-header__meta`">$dateHtml</div>" })
    $tagsHtml
</header>
"@
    }
    
    # Build full content
    $fullContent = @"
<div class="static-content container">
    $headerHtml
    <article class="prose">
        $HtmlContent
    </article>
</div>
"@
    
    # Replace title
    $html = [regex]::Replace($html, '<title>[^<]*</title>', "<title>$pageTitle</title>")
    
    # Add/update meta description
    if ($html -match '<meta\s+name="description"') {
        $html = [regex]::Replace($html, '<meta\s+name="description"\s+content="[^"]*"[^>]*>', "<meta name=`"description`" content=`"$metaDescription`" />")
    }
    else {
        $html = $html -replace '(<meta\s+name="viewport"[^>]*>)', "`$1`n    <meta name=`"description`" content=`"$metaDescription`" />"
    }
    
    # Add Open Graph tags (including og:image and og:logo)
    $ogImageTag = if ($ogImageUrl) { "`n    <meta property=`"og:image`" content=`"$ogImageUrl`" />" } else { "" }
    $ogLogoTag = if ($logoUrl) { "`n    <meta property=`"og:logo`" content=`"$logoUrl`" />" } else { "" }
    
    $ogTags = @"
    
    <!-- Open Graph (SSG) -->
    <meta property="og:title" content="$pageTitle" />
    <meta property="og:description" content="$metaDescription" />
    <meta property="og:type" content="$OgType" />
    <meta property="og:url" content="$CanonicalUrl" />
    <meta property="og:locale" content="en_US" />$ogImageTag$ogLogoTag
    <link rel="canonical" href="$CanonicalUrl" />
"@
    
    # Insert OG tags before </head>
    $html = $html -replace '</head>', "$ogTags`n</head>"
    
    # Replace loading content with actual content
    # The div#app will contain static content that Blazor will replace
    $loadingPattern = '<div id="app">[\s\S]*?</div>'
    $appContent = @"
<div id="app">
    <!-- Static content for SEO - Blazor will hydrate this -->
    $fullContent
    
    <!-- Loading indicator (shown briefly before Blazor loads) -->
    <noscript>
        <div style="padding: 2rem; text-align: center; background: var(--color-bg-primary); color: var(--color-text-primary);">
            <h1>JavaScript Required</h1>
            <p>This site requires JavaScript to function properly.</p>
            <p>Please enable JavaScript or visit our <a href="https://github.com/dmitrat" style="color: var(--color-accent);">GitHub</a>.</p>
        </div>
    </noscript>
</div>
"@
    
    $html = [regex]::Replace($html, $loadingPattern, $appContent)
    
    return $html
}

# Create directory if not exists
function Ensure-Directory {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

# Stats
$stats = @{
    blog     = 0
    projects = 0
    articles = 0
    docs     = 0
    pages    = 0
}

# Process blog posts
$blogPath = Join-Path $ContentPath "blog"
if (Test-Path $blogPath) {
    Write-Host "`n  Processing blog posts..." -ForegroundColor Gray
    
    $blogFiles = Get-ChildItem -Path $blogPath -Filter "*.md*" | Sort-Object Name -Descending
    foreach ($file in $blogFiles) {
        $slug = Get-SlugFromPath -Path $file.FullName
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $htmlContent = Convert-MarkdownToHtml -Markdown $parsed.content
        
        $desc = Coalesce $parsed.description $parsed.summary
        
        $pageHtml = Generate-StaticPage `
            -Title $parsed.title `
            -Description $desc `
            -HtmlContent $htmlContent `
            -CanonicalUrl "$siteUrl/blog/$slug" `
            -OgType "article" `
            -PublishDate $parsed.publishDate `
            -Tags $parsed.tags
        
        $outputDir = Join-Path $OutputPath "blog/$slug"
        Ensure-Directory -Path $outputDir
        $pageHtml | Out-File -FilePath (Join-Path $outputDir "index.html") -Encoding UTF8 -NoNewline
        
        $stats.blog++
        Write-Host "    + /blog/$slug" -ForegroundColor DarkGray
    }
}

# Process projects
$projectsPath = Join-Path $ContentPath "projects"
if (Test-Path $projectsPath) {
    Write-Host "`n  Processing projects..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $projectsPath | Sort-Object Name
    foreach ($item in $items) {
        $mdFile = $null
        if ($item.PSIsContainer) {
            $mdFile = Get-ChildItem -Path $item.FullName -Filter "index.md*" -File | Select-Object -First 1
            $slug = Get-SlugFromPath -Path $item.Name
        }
        else {
            $extension = [System.IO.Path]::GetExtension($item.Name).ToLower()
            if ($extension -eq ".md" -or $extension -eq ".mdx") {
                $mdFile = $item
                $slug = Get-SlugFromPath -Path $item.Name
            }
        }
        
        if ($mdFile) {
            $content = Get-Content $mdFile.FullName -Raw -Encoding UTF8
            $parsed = Parse-Frontmatter -Content $content
            $htmlContent = Convert-MarkdownToHtml -Markdown $parsed.content
            
            $desc = Coalesce $parsed.description $parsed.summary
            
            $pageHtml = Generate-StaticPage `
                -Title $parsed.title `
                -Description $desc `
                -HtmlContent $htmlContent `
                -CanonicalUrl "$siteUrl/project/$slug" `
                -OgType "article" `
                -Tags $parsed.tags
            
            $outputDir = Join-Path $OutputPath "project/$slug"
            Ensure-Directory -Path $outputDir
            $pageHtml | Out-File -FilePath (Join-Path $outputDir "index.html") -Encoding UTF8 -NoNewline
            
            $stats.projects++
            Write-Host "    + /project/$slug" -ForegroundColor DarkGray
        }
    }
}

# Process articles
$articlesPath = Join-Path $ContentPath "articles"
if (Test-Path $articlesPath) {
    Write-Host "`n  Processing articles..." -ForegroundColor Gray
    
    $articleFiles = Get-ChildItem -Path $articlesPath -Filter "*.md*" | Sort-Object Name
    foreach ($file in $articleFiles) {
        $slug = Get-SlugFromPath -Path $file.FullName
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $htmlContent = Convert-MarkdownToHtml -Markdown $parsed.content
        
        $desc = Coalesce $parsed.description $parsed.summary
        
        $pageHtml = Generate-StaticPage `
            -Title $parsed.title `
            -Description $desc `
            -HtmlContent $htmlContent `
            -CanonicalUrl "$siteUrl/article/$slug" `
            -OgType "article" `
            -PublishDate $parsed.publishDate `
            -Tags $parsed.tags
        
        $outputDir = Join-Path $OutputPath "article/$slug"
        Ensure-Directory -Path $outputDir
        $pageHtml | Out-File -FilePath (Join-Path $outputDir "index.html") -Encoding UTF8 -NoNewline
        
        $stats.articles++
        Write-Host "    + /article/$slug" -ForegroundColor DarkGray
    }
}

# Process docs
$docsPath = Join-Path $ContentPath "docs"
if (Test-Path $docsPath) {
    Write-Host "`n  Processing docs..." -ForegroundColor Gray
    
    $docFiles = Get-ChildItem -Path $docsPath -Filter "*.md*" | Sort-Object Name
    foreach ($file in $docFiles) {
        $slug = Get-SlugFromPath -Path $file.FullName
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $htmlContent = Convert-MarkdownToHtml -Markdown $parsed.content
        
        $desc = Coalesce $parsed.description $parsed.summary
        
        $pageHtml = Generate-StaticPage `
            -Title $parsed.title `
            -Description $desc `
            -HtmlContent $htmlContent `
            -CanonicalUrl "$siteUrl/docs/$slug" `
            -OgType "article"
        
        $outputDir = Join-Path $OutputPath "docs/$slug"
        Ensure-Directory -Path $outputDir
        $pageHtml | Out-File -FilePath (Join-Path $outputDir "index.html") -Encoding UTF8 -NoNewline
        
        $stats.docs++
        Write-Host "    + /docs/$slug" -ForegroundColor DarkGray
    }
}

# Generate static pages for main routes
Write-Host "`n  Generating static pages for main routes..." -ForegroundColor Gray

# Blog list page
$blogListHtml = Generate-StaticPage `
    -Title "Blog" `
    -Description "Read the latest articles about software development, .NET, and more." `
    -HtmlContent "<p>Loading blog posts...</p>" `
    -CanonicalUrl "$siteUrl/blog"
$blogDir = Join-Path $OutputPath "blog"
Ensure-Directory -Path $blogDir
$blogListHtml | Out-File -FilePath (Join-Path $blogDir "index.html") -Encoding UTF8 -NoNewline
$stats.pages++
Write-Host "    + /blog" -ForegroundColor DarkGray

# Contact page
$contactHtml = Generate-StaticPage `
    -Title "Contact" `
    -Description "Get in touch with me." `
    -HtmlContent "<h1>Contact</h1><p>Loading contact form...</p>" `
    -CanonicalUrl "$siteUrl/contact"
$contactDir = Join-Path $OutputPath "contact"
Ensure-Directory -Path $contactDir
$contactHtml | Out-File -FilePath (Join-Path $contactDir "index.html") -Encoding UTF8 -NoNewline
$stats.pages++
Write-Host "    + /contact" -ForegroundColor DarkGray

# Search page
$searchHtml = Generate-StaticPage `
    -Title "Search" `
    -Description "Search across all content." `
    -HtmlContent "<h1>Search</h1><p>Loading search...</p>" `
    -CanonicalUrl "$siteUrl/search"
$searchDir = Join-Path $OutputPath "search"
Ensure-Directory -Path $searchDir
$searchHtml | Out-File -FilePath (Join-Path $searchDir "index.html") -Encoding UTF8 -NoNewline
$stats.pages++
Write-Host "    + /search" -ForegroundColor DarkGray

# Generate hosting provider-specific files
Write-Host "`n  Generating hosting configuration..." -ForegroundColor Gray

$hostingFiles = @()

switch ($HostingProvider) {
    "cloudflare" {
        # Cloudflare Pages _headers
        $headersContent = @"
# Cloudflare Pages headers
# https://developers.cloudflare.com/pages/platform/headers

# Cache static assets
/_framework/*
  Cache-Control: public, max-age=31536000, immutable

/css/*
  Cache-Control: public, max-age=31536000, immutable

/images/*
  Cache-Control: public, max-age=86400

# HTML pages - allow revalidation
/*.html
  Cache-Control: public, max-age=0, must-revalidate

/*/index.html
  Cache-Control: public, max-age=0, must-revalidate

# Security headers
/*
  X-Content-Type-Options: nosniff
  X-Frame-Options: DENY
  Referrer-Policy: strict-origin-when-cross-origin
"@
        $headersContent | Out-File -FilePath (Join-Path $OutputPath "_headers") -Encoding UTF8 -NoNewline
        $hostingFiles += "_headers"

        # Cloudflare Pages _redirects (SPA fallback)
        $redirectsContent = @"
# Cloudflare Pages redirects
# SPA fallback - serve index.html for all routes not matching a file
/*  /index.html  200
"@
        $redirectsContent | Out-File -FilePath (Join-Path $OutputPath "_redirects") -Encoding UTF8 -NoNewline
        $hostingFiles += "_redirects"
    }

    "netlify" {
        # Netlify _headers
        $headersContent = @"
# Netlify headers
# https://docs.netlify.com/routing/headers/

_framework/*
  Cache-Control: public, max-age=31536000, immutable

/css/*
  Cache-Control: public, max-age=31536000, immutable

/images/*
  Cache-Control: public, max-age=86400

/*.html
  Cache-Control: public, max-age=0, must-revalidate

/*
  X-Content-Type-Options: nosniff
  X-Frame-Options: DENY
  Referrer-Policy: strict-origin-when-cross-origin
"@
        $headersContent | Out-File -FilePath (Join-Path $OutputPath "_headers") -Encoding UTF8 -NoNewline
        $hostingFiles += "_headers"

        # Netlify _redirects (SPA fallback)
        $redirectsContent = @"
# Netlify redirects
# SPA fallback
/*  /index.html  200
"@
        $redirectsContent | Out-File -FilePath (Join-Path $OutputPath "_redirects") -Encoding UTF8 -NoNewline
        $hostingFiles += "_redirects"
    }

    "vercel" {
        # Vercel vercel.json
        $vercelConfig = @{
            rewrites = @(
                @{ source = "/((?!_framework|css|images|content|.*\\..*).*)"; destination = "/index.html" }
            )
            headers = @(
                @{ source = "/_framework/(.*)"; headers = @(@{ key = "Cache-Control"; value = "public, max-age=31536000, immutable" }) }
                @{ source = "/css/(.*)"; headers = @(@{ key = "Cache-Control"; value = "public, max-age=31536000, immutable" }) }
                @{ source = "/images/(.*)"; headers = @(@{ key = "Cache-Control"; value = "public, max-age=86400" }) }
                @{ source = "/(.*)\\.html"; headers = @(@{ key = "Cache-Control"; value = "public, max-age=0, must-revalidate" }) }
            )
        }
        $vercelConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath (Join-Path $OutputPath "vercel.json") -Encoding UTF8 -NoNewline
        $hostingFiles += "vercel.json"
    }

    "github" {
        # GitHub Pages doesn't support custom redirects for SPA
        # But we can add a 404.html that redirects to index.html
        $notFoundHtml = $templateHtml
        $notFoundHtml = [regex]::Replace($notFoundHtml, '<title>[^<]*</title>', "<title>Page Not Found - $siteName</title>")
        
        # Add redirect script for SPA routing
        $redirectScript = @"
<script>
    // GitHub Pages SPA redirect
    var path = window.location.pathname;
    if (path !== '/' && path !== '/index.html') {
        sessionStorage.setItem('redirectPath', path);
        window.location.replace('/');
    }
</script>
"@
        $notFoundHtml = $notFoundHtml -replace '</head>', "$redirectScript`n</head>"
        $notFoundHtml | Out-File -FilePath (Join-Path $OutputPath "404.html") -Encoding UTF8 -NoNewline
        $hostingFiles += "404.html"

        # Add .nojekyll to prevent Jekyll processing
        "" | Out-File -FilePath (Join-Path $OutputPath ".nojekyll") -Encoding UTF8 -NoNewline
        $hostingFiles += ".nojekyll"
    }

    "none" {
        # No hosting-specific files
        Write-Host "    (skipped - no hosting provider specified)" -ForegroundColor DarkGray
    }
}

foreach ($file in $hostingFiles) {
    Write-Host "    + $file" -ForegroundColor DarkGray
}

# Summary
$totalPages = $stats.blog + $stats.projects + $stats.articles + $stats.docs + $stats.pages

Write-Host "`nDone! Generated $totalPages static HTML pages:" -ForegroundColor Cyan
Write-Host "  - Blog posts: $($stats.blog)"
Write-Host "  - Projects: $($stats.projects)"
Write-Host "  - Articles: $($stats.articles)"
Write-Host "  - Docs: $($stats.docs)"
Write-Host "  - Static pages: $($stats.pages)"
if ($hostingFiles.Count -gt 0) {
    Write-Host "  - Hosting files: $($hostingFiles -join ', ')"
}
