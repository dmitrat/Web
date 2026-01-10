<#
.SYNOPSIS
    Generate search index JSON for client-side search.
    
.DESCRIPTION
    Parses all markdown content and creates a pre-built search index.
    This eliminates the need to build the index on the client,
    improving initial load time and reducing memory usage.
    
.PARAMETER SitePath
    Path to the site folder (e.g., "sites/ratner.io")
    
.PARAMETER OutputPath
    Path to output folder (defaults to SitePath/wwwroot)

.EXAMPLE
    .\generate-search-index.ps1 -SitePath "sites/ratner.io"
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

Write-Host "Generating search index..." -ForegroundColor Cyan
Write-Host "  Content: $ContentPath"
Write-Host "  Output: $OutputPath"

# Simple YAML frontmatter parser
function Parse-Frontmatter {
    param([string]$Content)
    
    $result = @{
        title       = ""
        description = ""
        summary     = ""
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
        if ($yaml -match "tags:\s*\[([^\]]+)\]") {
            $result.tags = $Matches[1] -split "," | ForEach-Object { $_.Trim().Trim("'", '"') }
        }
    }
    
    return $result
}

# Extract plain text from markdown (remove syntax)
function Extract-PlainText {
    param([string]$Markdown)
    
    $text = $Markdown
    
    # Remove code blocks
    $text = [regex]::Replace($text, '```[\s\S]*?```', ' ')
    
    # Remove inline code
    $text = [regex]::Replace($text, '`[^`]+`', ' ')
    
    # Remove links but keep text
    $text = [regex]::Replace($text, '\[([^\]]+)\]\([^)]+\)', '$1')
    
    # Remove images
    $text = [regex]::Replace($text, '!\[([^\]]*)\]\([^)]+\)', ' ')
    
    # Remove headers markers
    $text = [regex]::Replace($text, '^#{1,6}\s+', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove bold/italic markers
    $text = [regex]::Replace($text, '\*\*\*(.+?)\*\*\*', '$1')
    $text = [regex]::Replace($text, '\*\*(.+?)\*\*', '$1')
    $text = [regex]::Replace($text, '\*(.+?)\*', '$1')
    $text = [regex]::Replace($text, '___(.+?)___', '$1')
    $text = [regex]::Replace($text, '__(.+?)__', '$1')
    $text = [regex]::Replace($text, '_(.+?)_', '$1')
    
    # Remove blockquotes
    $text = [regex]::Replace($text, '^>\s+', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove horizontal rules
    $text = [regex]::Replace($text, '^---+$', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove list markers
    $text = [regex]::Replace($text, '^\s*[-*+]\s+', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $text = [regex]::Replace($text, '^\s*\d+\.\s+', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove HTML tags
    $text = [regex]::Replace($text, '<[^>]+>', ' ')
    
    # Normalize whitespace
    $text = [regex]::Replace($text, '\s+', ' ')
    
    return $text.Trim()
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

# Helper function for null coalescing
function Coalesce {
    param($a, $b)
    if ($a) { return $a } else { return $b }
}

# Search index entries
$searchIndex = @()

# Process blog posts
$blogPath = Join-Path $ContentPath "blog"
if (Test-Path $blogPath) {
    Write-Host "  Indexing blog posts..." -ForegroundColor Gray
    
    $blogFiles = Get-ChildItem -Path $blogPath -Filter "*.md*" | Sort-Object Name -Descending
    foreach ($file in $blogFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $file.FullName
        $plainText = Extract-PlainText -Markdown $parsed.content
        
        # Use FULL text for search, not truncated
        # Limit to reasonable size to prevent huge index (10KB per entry max)
        $maxContentLength = 10000
        $searchContent = if ($plainText.Length -gt $maxContentLength) { 
            $plainText.Substring(0, $maxContentLength) 
        } else { 
            $plainText 
        }
        
        $searchIndex += @{
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description
            content     = $searchContent
            url         = "/blog/$slug"
            type        = "blog"
            tags        = $parsed.tags
        }
    }
    Write-Host "    Indexed $($blogFiles.Count) blog posts" -ForegroundColor Green
}

# Process projects
$projectsPath = Join-Path $ContentPath "projects"
if (Test-Path $projectsPath) {
    Write-Host "  Indexing projects..." -ForegroundColor Gray
    
    $projectCount = 0
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
            $plainText = Extract-PlainText -Markdown $parsed.content
            
            $maxContentLength = 10000
            $searchContent = if ($plainText.Length -gt $maxContentLength) { 
                $plainText.Substring(0, $maxContentLength) 
            } else { 
                $plainText 
            }
            
            $searchIndex += @{
                title       = $parsed.title
                description = Coalesce $parsed.summary $parsed.description
                content     = $searchContent
                url         = "/project/$slug"
                type        = "project"
                tags        = $parsed.tags
            }
            $projectCount++
        }
    }
    Write-Host "    Indexed $projectCount projects" -ForegroundColor Green
}

# Process articles
$articlesPath = Join-Path $ContentPath "articles"
if (Test-Path $articlesPath) {
    Write-Host "  Indexing articles..." -ForegroundColor Gray
    
    $articleFiles = Get-ChildItem -Path $articlesPath -Filter "*.md*" | Sort-Object Name
    foreach ($file in $articleFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $file.FullName
        $plainText = Extract-PlainText -Markdown $parsed.content
        
        $maxContentLength = 10000
        $searchContent = if ($plainText.Length -gt $maxContentLength) { 
            $plainText.Substring(0, $maxContentLength) 
        } else { 
            $plainText 
        }
        
        $searchIndex += @{
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description
            content     = $searchContent
            url         = "/article/$slug"
            type        = "article"
            tags        = $parsed.tags
        }
    }
    Write-Host "    Indexed $($articleFiles.Count) articles" -ForegroundColor Green
}

# Process docs
$docsPath = Join-Path $ContentPath "docs"
if (Test-Path $docsPath) {
    Write-Host "  Indexing docs..." -ForegroundColor Gray
    
    $docFiles = Get-ChildItem -Path $docsPath -Filter "*.md*" | Sort-Object Name
    foreach ($file in $docFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $file.FullName
        $plainText = Extract-PlainText -Markdown $parsed.content
        
        $maxContentLength = 10000
        $searchContent = if ($plainText.Length -gt $maxContentLength) { 
            $plainText.Substring(0, $maxContentLength) 
        } else { 
            $plainText 
        }
        
        $searchIndex += @{
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description
            content     = $searchContent
            url         = "/docs/$slug"
            type        = "docs"
            tags        = $parsed.tags
        }
    }
    Write-Host "    Indexed $($docFiles.Count) docs" -ForegroundColor Green
}

# Write search-index.json
$indexPath = Join-Path $OutputPath "search-index.json"

# Convert to JSON with camelCase property names
$jsonIndex = $searchIndex | ForEach-Object {
    [PSCustomObject]@{
        title       = $_.title
        description = $_.description
        content     = $_.content
        url         = $_.url
        type        = $_.type
        tags        = $_.tags
    }
}

$jsonIndex | ConvertTo-Json -Depth 10 -Compress | Out-File -FilePath $indexPath -Encoding UTF8 -NoNewline

$indexSize = (Get-Item $indexPath).Length
$indexSizeKb = [math]::Round($indexSize / 1024, 1)

Write-Host ""
Write-Host "Done! Generated search-index.json" -ForegroundColor Cyan
Write-Host "  - Entries: $($searchIndex.Count)"
Write-Host "  - Size: $indexSizeKb KB"
Write-Host "  - Path: $indexPath"
