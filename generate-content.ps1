<#
.SYNOPSIS
    Generate content index, sitemap, and robots.txt for the website.
    
.DESCRIPTION
    Scans content folders (projects, blog, articles) and generates:
    - index.json - content manifest for Blazor ContentService
    - sitemap.xml - SEO sitemap for search engines
    - robots.txt - search engine crawling rules
    
.PARAMETER SitePath
    Path to the site folder (e.g., "sites/ratner.io")
    If provided, ContentPath and OutputPath are auto-detected,
    and SiteUrl is read from site.config.json
    
.PARAMETER ContentPath
    Path to wwwroot/content folder (optional if SitePath provided)
    
.PARAMETER OutputPath
    Path to output folder (optional if SitePath provided)
    
.PARAMETER SiteUrl
    Base URL for sitemap (optional if SitePath provided - reads from site.config.json)
    
.EXAMPLE
    # Simple: just specify site folder
    .\generate-content.ps1 -SitePath "sites/ratner.io"
    
.EXAMPLE
    # Advanced: specify all paths manually
    .\generate-content.ps1 -ContentPath "wwwroot/content" -OutputPath "wwwroot" -SiteUrl "https://example.com"
#>

param(
    [string]$SitePath,
    [string]$ContentPath,
    [string]$OutputPath,
    [string]$SiteUrl
)

$ErrorActionPreference = "Stop"

# If SitePath is provided, auto-detect other paths
if ($SitePath) {
    $SitePath = $SitePath.TrimEnd("/", "\")
    
    if (-not $ContentPath) {
        $ContentPath = Join-Path $SitePath "wwwroot/content"
    }
    if (-not $OutputPath) {
        $OutputPath = Join-Path $SitePath "wwwroot"
    }
    if (-not $SiteUrl) {
        $configPath = Join-Path $SitePath "wwwroot/site.config.json"
        if (Test-Path $configPath) {
            $config = Get-Content $configPath -Raw | ConvertFrom-Json
            $SiteUrl = $config.baseUrl
            Write-Host "  Read SiteUrl from site.config.json: $SiteUrl" -ForegroundColor Yellow
        }
        else {
            throw "site.config.json not found at $configPath. Please specify -SiteUrl manually."
        }
    }
}

# Validate required parameters
if (-not $ContentPath) {
    throw "ContentPath is required. Use -SitePath or -ContentPath parameter."
}
if (-not $OutputPath) {
    throw "OutputPath is required. Use -SitePath or -OutputPath parameter."
}
if (-not $SiteUrl) {
    throw "SiteUrl is required. Use -SitePath (to read from site.config.json) or -SiteUrl parameter."
}

Write-Host "Generating content index, sitemap, and robots.txt..." -ForegroundColor Cyan
Write-Host "  Content Path: $ContentPath"
Write-Host "  Output Path: $OutputPath"
Write-Host "  Site URL: $SiteUrl"

# Initialize index structure (matches ContentService expectations)
$index = @{
    blog     = @()
    projects = @()
    docs     = @()
    articles = @()
    features = @()
}

# Initialize sitemap entries
$sitemapEntries = @()

# Add static pages to sitemap
$staticPages = @(
    @{ url = "/"; priority = "1.0" },
    @{ url = "/blog"; priority = "0.8" },
    @{ url = "/contact"; priority = "0.5" }
)
foreach ($page in $staticPages) {
    $sitemapEntries += @{
        url      = "$SiteUrl$($page.url)"
        lastmod  = (Get-Date -Format "yyyy-MM-dd")
        priority = $page.priority
    }
}

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

# Scan projects
$projectsPath = Join-Path $ContentPath "projects"
if (Test-Path $projectsPath) {
    Write-Host "  Scanning projects..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $projectsPath | Sort-Object Name
    foreach ($item in $items) {
        if ($item.PSIsContainer) {
            $indexFile = Get-ChildItem -Path $item.FullName -Filter "index.md*" -File | Select-Object -First 1
            if ($indexFile) {
                $index.projects += "$($item.Name)/$($indexFile.Name)"
                $slug = Get-SlugFromPath -Path $item.Name
                $sitemapEntries += @{
                    url      = "$SiteUrl/project/$slug"
                    lastmod  = $item.LastWriteTime.ToString("yyyy-MM-dd")
                    priority = "0.7"
                }
            }
        }
        else {
            $extension = [System.IO.Path]::GetExtension($item.Name).ToLower()
            if ($extension -eq ".md" -or $extension -eq ".mdx") {
                $index.projects += $item.Name
                $slug = Get-SlugFromPath -Path $item.Name
                $sitemapEntries += @{
                    url      = "$SiteUrl/project/$slug"
                    lastmod  = $item.LastWriteTime.ToString("yyyy-MM-dd")
                    priority = "0.7"
                }
            }
        }
    }
    Write-Host "    Found $($index.projects.Count) projects" -ForegroundColor Green
}

# Scan blog
$blogPath = Join-Path $ContentPath "blog"
if (Test-Path $blogPath) {
    Write-Host "  Scanning blog posts..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $blogPath -Filter "*.md*" | Sort-Object Name -Descending
    foreach ($item in $items) {
        $index.blog += $item.Name
        $slug = Get-SlugFromPath -Path $item.Name
        $sitemapEntries += @{
            url      = "$SiteUrl/blog/$slug"
            lastmod  = $item.LastWriteTime.ToString("yyyy-MM-dd")
            priority = "0.6"
        }
    }
    Write-Host "    Found $($index.blog.Count) blog posts" -ForegroundColor Green
}

# Scan articles
$articlesPath = Join-Path $ContentPath "articles"
if (Test-Path $articlesPath) {
    Write-Host "  Scanning articles..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $articlesPath -Filter "*.md*" | Sort-Object Name
    foreach ($item in $items) {
        $index.articles += $item.Name
        $slug = Get-SlugFromPath -Path $item.Name
        $sitemapEntries += @{
            url      = "$SiteUrl/article/$slug"
            lastmod  = $item.LastWriteTime.ToString("yyyy-MM-dd")
            priority = "0.6"
        }
    }
    Write-Host "    Found $($index.articles.Count) articles" -ForegroundColor Green
}

# Scan docs
$docsPath = Join-Path $ContentPath "docs"
if (Test-Path $docsPath) {
    Write-Host "  Scanning docs..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $docsPath -Filter "*.md*" | Sort-Object Name
    foreach ($item in $items) {
        $index.docs += $item.Name
    }
    Write-Host "    Found $($index.docs.Count) docs" -ForegroundColor Green
}

# Scan features
$featuresPath = Join-Path $ContentPath "features"
if (Test-Path $featuresPath) {
    Write-Host "  Scanning features..." -ForegroundColor Gray
    
    $items = Get-ChildItem -Path $featuresPath -Filter "*.md*" | Sort-Object Name
    foreach ($item in $items) {
        $index.features += $item.Name
    }
    Write-Host "    Found $($index.features.Count) features" -ForegroundColor Green
}

# Write index.json (content manifest for ContentService)
$indexPath = Join-Path $OutputPath "content/index.json"
$indexDir = Split-Path $indexPath -Parent
if (-not (Test-Path $indexDir)) {
    New-Item -ItemType Directory -Path $indexDir -Force | Out-Null
}
$index | ConvertTo-Json -Depth 10 | Out-File -FilePath $indexPath -Encoding UTF8 -NoNewline
Write-Host "  Created: $indexPath" -ForegroundColor Green

# Write sitemap.xml
$sitemapPath = Join-Path $OutputPath "sitemap.xml"
$sitemapXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
"@

foreach ($entry in $sitemapEntries) {
    $sitemapXml += @"

    <url>
        <loc>$($entry.url)</loc>
        <lastmod>$($entry.lastmod)</lastmod>
        <priority>$($entry.priority)</priority>
    </url>
"@
}

$sitemapXml += @"

</urlset>
"@

$sitemapXml | Out-File -FilePath $sitemapPath -Encoding UTF8 -NoNewline
Write-Host "  Created: $sitemapPath ($($sitemapEntries.Count) URLs)" -ForegroundColor Green

# Write robots.txt
$robotsPath = Join-Path $OutputPath "robots.txt"
$robotsTxt = @"
# robots.txt for $SiteUrl
User-agent: *
Allow: /

# Sitemap
Sitemap: $SiteUrl/sitemap.xml
"@

$robotsTxt | Out-File -FilePath $robotsPath -Encoding UTF8 -NoNewline
Write-Host "  Created: $robotsPath" -ForegroundColor Green

Write-Host ""
Write-Host "Done! Generated:" -ForegroundColor Cyan
Write-Host "  - index.json ($($index.projects.Count) projects, $($index.blog.Count) blog, $($index.articles.Count) articles, $($index.features.Count) features)"
Write-Host "  - sitemap.xml ($($sitemapEntries.Count) URLs)"
Write-Host "  - robots.txt"
