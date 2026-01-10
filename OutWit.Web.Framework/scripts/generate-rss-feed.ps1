<#
.SYNOPSIS
    Generate RSS feed for blog posts.
    
.DESCRIPTION
    Parses blog markdown files and creates an RSS 2.0 feed.
    
.PARAMETER SitePath
    Path to the site folder (e.g., "sites/ratner.io")
    
.PARAMETER OutputPath
    Path to output folder (defaults to SitePath/wwwroot)

.EXAMPLE
    .\generate-rss-feed.ps1 -SitePath "sites/ratner.io"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SitePath,
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

# Normalize paths
$SitePath = $SitePath.TrimEnd("/", "\")
if (-not $OutputPath) {
    $OutputPath = Join-Path $SitePath "wwwroot"
}

$ContentPath = Join-Path $OutputPath "content"
$ConfigPath = Join-Path $OutputPath "site.config.json"

# Validate paths
if (-not (Test-Path $ConfigPath)) {
    throw "site.config.json not found at $ConfigPath"
}

# Load configuration
$config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$siteUrl = $config.baseUrl.TrimEnd('/')
$siteName = $config.siteName
$siteDescription = if ($config.seo.description) { $config.seo.description } else { "Latest posts from $siteName" }

Write-Host "Generating RSS feed..." -ForegroundColor Cyan
Write-Host "  Site: $siteName ($siteUrl)"

# Simple YAML frontmatter parser
function Parse-Frontmatter {
    param([string]$Content)
    
    $result = @{
        title       = ""
        description = ""
        summary     = ""
        publishDate = $null
        author      = ""
        content     = $Content
    }
    
    if ($Content -match "^---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n([\s\S]*)$") {
        $yaml = $Matches[1]
        $result.content = $Matches[2]
        
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
        if ($yaml -match "author:\s*['""]?([^'""'\r\n]+)['""]?") {
            $result.author = $Matches[1].Trim()
        }
    }
    
    return $result
}

# Generate slug from filename
function Get-SlugFromPath {
    param([string]$Path)
    
    $name = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    
    # Remove date prefix for blog posts (2024-11-20-title -> title)
    if ($name -match "^\d{4}-\d{2}-\d{2}-") {
        $name = $name -replace "^\d{4}-\d{2}-\d{2}-", ""
    }
    
    return $name
}

# Helper for null coalescing
function Coalesce {
    param($a, $b)
    if ($a) { return $a } else { return $b }
}

# Escape XML special characters
function Escape-Xml {
    param([string]$Text)
    if (-not $Text) { return "" }
    return $Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace('"', "&quot;").Replace("'", "&apos;")
}

# Collect blog posts
$posts = @()

$blogPath = Join-Path $ContentPath "blog"
if (Test-Path $blogPath) {
    $blogFiles = Get-ChildItem -Path $blogPath -Filter "*.md*" | Sort-Object Name -Descending
    
    foreach ($file in $blogFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $file.FullName
        
        $posts += @{
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description
            link        = "$siteUrl/blog/$slug"
            pubDate     = if ($parsed.publishDate) { $parsed.publishDate } else { $file.LastWriteTime }
            author      = $parsed.author
            guid        = "$siteUrl/blog/$slug"
        }
    }
}

Write-Host "  Found $($posts.Count) blog posts" -ForegroundColor Green

# Generate RSS XML
$rssXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
  <channel>
    <title>$(Escape-Xml $siteName)</title>
    <link>$siteUrl</link>
    <description>$(Escape-Xml $siteDescription)</description>
    <language>en-us</language>
    <lastBuildDate>$((Get-Date).ToString("R"))</lastBuildDate>
    <atom:link href="$siteUrl/feed.xml" rel="self" type="application/rss+xml"/>

"@

foreach ($post in $posts) {
    $pubDateStr = $post.pubDate.ToString("R")
    $rssXml += @"
    <item>
      <title>$(Escape-Xml $post.title)</title>
      <link>$($post.link)</link>
      <description>$(Escape-Xml $post.description)</description>
      <pubDate>$pubDateStr</pubDate>
      <guid isPermaLink="true">$($post.guid)</guid>
"@
    if ($post.author) {
        $rssXml += "      <author>$(Escape-Xml $post.author)</author>`n"
    }
    $rssXml += "    </item>`n"
}

$rssXml += @"
  </channel>
</rss>
"@

# Write feed.xml
$feedPath = Join-Path $OutputPath "feed.xml"
$rssXml | Out-File -FilePath $feedPath -Encoding UTF8 -NoNewline

Write-Host ""
Write-Host "Done! Generated feed.xml" -ForegroundColor Cyan
Write-Host "  - Posts: $($posts.Count)"
Write-Host "  - Path: $feedPath"
