<#
.SYNOPSIS
    Generate Open Graph preview images for all content pages.
    
.DESCRIPTION
    Creates 1200x630 PNG images for social media sharing.
    Uses Playwright to render HTML templates to images.
    
.PARAMETER SitePath
    Path to the site folder (e.g., "sites/ratner.io")
    
.PARAMETER OutputPath
    Path to output folder for images (defaults to wwwroot/og-images)

.PARAMETER TemplatePath
    Path to HTML template for OG images (optional, uses default)

.PARAMETER Force
    Regenerate all images even if they exist

.EXAMPLE
    .\generate-og-images.ps1 -SitePath "sites/ratner.io"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SitePath,
    [string]$OutputPath,
    [string]$TemplatePath,
    [switch]$Force
)

# Don't stop on errors - we handle them gracefully
$ErrorActionPreference = "Continue"

# Normalize paths - convert to absolute
try {
    $SitePath = (Resolve-Path $SitePath.TrimEnd("/", "\")).Path
} catch {
    Write-Host "  Error: Site path not found: $SitePath" -ForegroundColor Red
    exit 0  # Exit gracefully - OG images are optional
}

$wwwroot = Join-Path $SitePath "wwwroot"

if (-not $OutputPath) {
    $OutputPath = Join-Path $wwwroot "og-images"
}

$ContentPath = Join-Path $wwwroot "content"
$ConfigPath = Join-Path $wwwroot "site.config.json"
$ThemeCssPath = Join-Path $wwwroot "css/theme.css"

# Validate paths
if (-not (Test-Path $ConfigPath)) {
    Write-Host "  Warning: site.config.json not found at $ConfigPath - skipping OG image generation" -ForegroundColor Yellow
    exit 0
}

# Load configuration
$config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$siteName = $config.siteName
$siteUrl = $config.baseUrl.TrimEnd('/')

# Function to extract CSS variable value from theme.css
function Get-CssVariable {
    param(
        [string]$CssContent,
        [string]$VariableName,
        [string]$Section,
        [string]$Default
    )
    
    $sectionContent = $CssContent
    
    # If section is specified, try to find that section first
    if ($Section) {
        # Match section like [data-theme="dark"] { ... }
        if ($CssContent -match "\[$Section\]\s*\{([^}]+)\}") {
            $sectionContent = $Matches[1]
        }
        # Also try :root for light theme
        elseif ($Section -eq 'data-theme="light"' -and $CssContent -match ":root\s*\{([^}]+)\}") {
            $sectionContent = $Matches[1]
        }
    }
    
    # Match pattern: --variable-name: #hexcolor; or --variable-name: value;
    if ($sectionContent -match "--$VariableName\s*:\s*([^;]+);") {
        return $Matches[1].Trim()
    }
    return $Default
}

# Read colors from theme.css
$accentColor = "#39FF14"  # default green
$bgColor = "#0D1626"      # default dark blue

if (Test-Path $ThemeCssPath) {
    $themeCss = Get-Content $ThemeCssPath -Raw
    $defaultTheme = if ($config.defaultTheme) { $config.defaultTheme } else { "dark" }
    
    # Determine which CSS section to read from
    $cssSection = if ($defaultTheme -eq "dark") { 'data-theme="dark"' } else { 'data-theme="light"' }
    
    Write-Host "  Default theme: $defaultTheme" -ForegroundColor Gray
    
    # Try to find accent color - priority: color-accent > color-accent-blue > color-accent-green
    $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent" -Section $cssSection -Default $null
    if (-not $foundAccent) {
        $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent-blue" -Section $cssSection -Default $null
    }
    if (-not $foundAccent) {
        $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent-green" -Section $cssSection -Default $null
    }
    # Fallback to :root if not found in theme section
    if (-not $foundAccent) {
        $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent" -Section $null -Default $null
    }
    if (-not $foundAccent) {
        $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent-blue" -Section $null -Default $null
    }
    if (-not $foundAccent) {
        $foundAccent = Get-CssVariable -CssContent $themeCss -VariableName "color-accent-green" -Section $null -Default $null
    }
    if ($foundAccent) {
        $accentColor = $foundAccent
    }
    
    # Try to find background color from theme section
    $foundBg = Get-CssVariable -CssContent $themeCss -VariableName "color-background" -Section $cssSection -Default $null
    # Fallback to :root if not found
    if (-not $foundBg) {
        $foundBg = Get-CssVariable -CssContent $themeCss -VariableName "color-background" -Section $null -Default $null
    }
    if ($foundBg) {
        $bgColor = $foundBg
    }
    
    Write-Host "  Theme colors from CSS: accent=$accentColor, bg=$bgColor" -ForegroundColor Gray
}
else {
    Write-Host "  theme.css not found, using default colors" -ForegroundColor Yellow
}

Write-Host "Generating Open Graph images..." -ForegroundColor Cyan
Write-Host "  Site: $siteName ($siteUrl)"

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Create temp directory for HTML templates (cross-platform)
$tempBase = if ($env:TEMP) { $env:TEMP } elseif ($env:TMPDIR) { $env:TMPDIR } else { "/tmp" }
$tempDir = Join-Path $tempBase "og-images-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

# HTML template for OG image
$htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            width: 1200px;
            height: 630px;
            background: linear-gradient(135deg, {{BG_COLOR}} 0%, {{BG_COLOR_DARK}} 100%);
            font-family: 'Segoe UI', 'Inter', -apple-system, sans-serif;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            padding: 60px;
            color: #F5F7FA;
        }
        .content {
            flex: 1;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }
        .type {
            font-size: 24px;
            text-transform: uppercase;
            letter-spacing: 3px;
            color: {{ACCENT_COLOR}};
            margin-bottom: 20px;
            font-weight: 600;
        }
        .title {
            font-size: 64px;
            font-weight: 700;
            line-height: 1.1;
            margin-bottom: 24px;
            max-width: 900px;
            display: -webkit-box;
            -webkit-line-clamp: 3;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }
        .description {
            font-size: 28px;
            color: #A9B4C2;
            max-width: 800px;
            line-height: 1.4;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }
        .footer {
            display: flex;
            align-items: center;
            justify-content: space-between;
        }
        .site-name {
            font-size: 28px;
            font-weight: 600;
            color: {{ACCENT_COLOR}};
        }
        .url {
            font-size: 22px;
            color: #6B7A8F;
        }
        .accent-bar {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            height: 8px;
            background: {{ACCENT_COLOR}};
        }
    </style>
</head>
<body>
    <div class="content">
        <div class="type">{{TYPE}}</div>
        <h1 class="title">{{TITLE}}</h1>
        <p class="description">{{DESCRIPTION}}</p>
    </div>
    <div class="footer">
        <span class="site-name">{{SITE_NAME}}</span>
        <span class="url">{{URL}}</span>
    </div>
    <div class="accent-bar"></div>
</body>
</html>
"@

# Function to darken a hex color
function Get-DarkerColor {
    param([string]$hex)
    $hex = $hex.TrimStart('#')
    if ($hex.Length -lt 6) { return "#000000" }
    try {
        $r = [Math]::Max(0, [Convert]::ToInt32($hex.Substring(0, 2), 16) - 20)
        $g = [Math]::Max(0, [Convert]::ToInt32($hex.Substring(2, 2), 16) - 20)
        $b = [Math]::Max(0, [Convert]::ToInt32($hex.Substring(4, 2), 16) - 20)
        return "#{0:X2}{1:X2}{2:X2}" -f $r, $g, $b
    } catch {
        return "#000000"
    }
}

# Simple YAML frontmatter parser
function Parse-Frontmatter {
    param([string]$Content)
    
    $result = @{
        title       = ""
        description = ""
        summary     = ""
    }
    
    if ($Content -match "^---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n") {
        $yaml = $Matches[1]
        
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
    }
    
    return $result
}

# Generate slug from filename
function Get-SlugFromPath {
    param([string]$Path)
    
    $name = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    
    if ($name -eq "index") {
        $parent = Split-Path -Path $Path -Parent
        if ($parent) {
            $name = Split-Path -Path $parent -Leaf
        }
    }
    
    if ($name -match "^\d{4}-\d{2}-\d{2}-") {
        $name = $name -replace "^\d{4}-\d{2}-\d{2}-", ""
    }
    elseif ($name -match "^\d+-") {
        $name = $name -replace "^\d+-", ""
    }
    
    return $name
}

# Escape HTML
function Escape-Html {
    param([string]$Text)
    if (-not $Text) { return "" }
    return $Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace('"', "&quot;")
}

# Strip markdown formatting from text (for plain text display)
function Strip-Markdown {
    param([string]$Text)
    if (-not $Text) { return "" }
    
    $result = $Text
    
    # Remove bold/italic: **text**, *text*, __text__, _text_
    $result = [regex]::Replace($result, '\*\*\*(.+?)\*\*\*', '$1')
    $result = [regex]::Replace($result, '\*\*(.+?)\*\*', '$1')
    $result = [regex]::Replace($result, '\*(.+?)\*', '$1')
    $result = [regex]::Replace($result, '___(.+?)___', '$1')
    $result = [regex]::Replace($result, '__(.+?)__', '$1')
    $result = [regex]::Replace($result, '_(.+?)_', '$1')
    
    # Remove inline code: `code`
    $result = [regex]::Replace($result, '`([^`]+)`', '$1')
    
    # Remove links: [text](url) -> text
    $result = [regex]::Replace($result, '\[([^\]]+)\]\([^)]+\)', '$1')
    
    # Remove images: ![alt](url)
    $result = [regex]::Replace($result, '!\[([^\]]*)\]\([^)]+\)', '$1')
    
    # Remove headers: # ## ### etc
    $result = [regex]::Replace($result, '^#{1,6}\s+', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove blockquotes: >
    $result = [regex]::Replace($result, '^>\s*', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove horizontal rules
    $result = [regex]::Replace($result, '^[-*_]{3,}$', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Normalize whitespace
    $result = [regex]::Replace($result, '\s+', ' ')
    
    return $result.Trim()
}

# Helper for null coalescing
function Coalesce {
    param($a, $b, $c)
    if ($a) { return $a }
    if ($b) { return $b }
    return $c
}

# Collect all pages to generate
$pages = @()
$bgColorDark = Get-DarkerColor -hex $bgColor

# Blog posts
$blogPath = Join-Path $ContentPath "blog"
if (Test-Path $blogPath) {
    Get-ChildItem -Path $blogPath -Filter "*.md*" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $_.FullName
        
        $pages += @{
            type        = "Blog"
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description ""
            slug        = $slug
            url         = "/blog/$slug"
            filename    = "blog-$slug.png"
        }
    }
}

# Projects
$projectsPath = Join-Path $ContentPath "projects"
if (Test-Path $projectsPath) {
    Get-ChildItem -Path $projectsPath -Recurse -Filter "*.md*" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $_.FullName
        
        $pages += @{
            type        = "Project"
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description ""
            slug        = $slug
            url         = "/project/$slug"
            filename    = "project-$slug.png"
        }
    }
}

# Articles
$articlesPath = Join-Path $ContentPath "articles"
if (Test-Path $articlesPath) {
    Get-ChildItem -Path $articlesPath -Filter "*.md*" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $_.FullName
        
        $pages += @{
            type        = "Article"
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description ""
            slug        = $slug
            url         = "/article/$slug"
            filename    = "article-$slug.png"
        }
    }
}

# Docs
$docsPath = Join-Path $ContentPath "docs"
if (Test-Path $docsPath) {
    Get-ChildItem -Path $docsPath -Filter "*.md*" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        $parsed = Parse-Frontmatter -Content $content
        $slug = Get-SlugFromPath -Path $_.FullName
        
        $pages += @{
            type        = "Documentation"
            title       = $parsed.title
            description = Coalesce $parsed.summary $parsed.description ""
            slug        = $slug
            url         = "/docs/$slug"
            filename    = "docs-$slug.png"
        }
    }
}

# Add default OG image for homepage
$pages += @{
    type        = ""
    title       = $siteName
    description = if ($config.seo.description) { $config.seo.description } else { "Welcome to $siteName" }
    slug        = "default"
    url         = $siteUrl
    filename    = "default.png"
}

Write-Host "  Found $($pages.Count) pages to process" -ForegroundColor Green

# Check if Playwright is available
$playwrightAvailable = $false
$npxCmd = $null

# Detect OS - use automatic variable $IsWindows (PowerShell Core) or check $env:OS (Windows PowerShell)
# Note: $IsWindows is read-only in PowerShell Core, so we use a different variable name
$runningOnWindows = if ($null -ne $IsWindows) { $IsWindows } else { $env:OS -eq "Windows_NT" }

# Try to find npx
try {
    if ($runningOnWindows) {
        # On Windows, prefer npx.cmd over npx.ps1 for ProcessStartInfo compatibility
        $nodeCmd = Get-Command node -ErrorAction SilentlyContinue
        if ($nodeCmd) {
            $npxCmdPath = Join-Path (Split-Path $nodeCmd.Source) "npx.cmd"
            if (Test-Path $npxCmdPath) {
                $npxCmd = $npxCmdPath
            }
        }
        
        if (-not $npxCmd) {
            # Fallback to whatever npx is available
            $npxSource = (Get-Command npx -ErrorAction SilentlyContinue).Source
            if ($npxSource -and $npxSource -notmatch '\.ps1$') {
                $npxCmd = $npxSource
            }
            elseif ($npxSource) {
                # npx.ps1 found - we'll use PowerShell invocation instead
                $npxCmd = "npx"
            }
        }
    }
    else {
        # On Linux/macOS, npx is a shell script or binary
        $npxSource = (Get-Command npx -ErrorAction SilentlyContinue).Source
        if ($npxSource) {
            $npxCmd = $npxSource
        }
    }
    
    if ($npxCmd) {
        Write-Host "  npx found: $npxCmd" -ForegroundColor Gray
    }
} catch {
    Write-Host "  npx not found in PATH" -ForegroundColor Yellow
}

if ($npxCmd) {
    # Check in site's node_modules
    $nodeModulesPath = Join-Path $SitePath "node_modules"
    $playwrightPath = Join-Path $nodeModulesPath "playwright"

    if (Test-Path $playwrightPath) {
        $playwrightAvailable = $true
        Write-Host "  Playwright found in site folder" -ForegroundColor Gray
    }

    # Check in repo root (parent of site folder)
    if (-not $playwrightAvailable) {
        $repoRoot = Split-Path -Parent (Split-Path -Parent $SitePath)
        $repoNodeModules = Join-Path (Join-Path $repoRoot "node_modules") "playwright"
        if (Test-Path $repoNodeModules) {
            $playwrightAvailable = $true
            Write-Host "  Playwright found in repo root" -ForegroundColor Gray
        }
    }

    # Check in current working directory
    if (-not $playwrightAvailable) {
        $cwdPath = (Get-Location).Path
        if ($cwdPath) {
            $cwdNodeModules = Join-Path (Join-Path $cwdPath "node_modules") "playwright"
            if (Test-Path $cwdNodeModules) {
                $playwrightAvailable = $true
                Write-Host "  Playwright found in current directory" -ForegroundColor Gray
            }
        }
    }

    # Check globally via npm
    if (-not $playwrightAvailable) {
        try {
            $npmRoot = & npm root -g 2>$null
            if ($npmRoot -and (Test-Path (Join-Path $npmRoot "playwright"))) {
                $playwrightAvailable = $true
                Write-Host "  Playwright found globally (npm)" -ForegroundColor Gray
            }
        }
        catch { }
    }
    
    # Check Playwright cache directory (used by `npx playwright install`)
    if (-not $playwrightAvailable) {
        $playwrightCachePaths = @()
        
        if ($runningOnWindows) {
            # Windows: %LOCALAPPDATA%\ms-playwright or %USERPROFILE%\.cache\ms-playwright
            if ($env:LOCALAPPDATA) {
                $playwrightCachePaths += Join-Path $env:LOCALAPPDATA "ms-playwright"
            }
            if ($env:USERPROFILE) {
                $playwrightCachePaths += Join-Path (Join-Path $env:USERPROFILE ".cache") "ms-playwright"
            }
        }
        else {
            # Linux/macOS: ~/.cache/ms-playwright or $HOME/.cache/ms-playwright
            if ($env:HOME) {
                $playwrightCachePaths += Join-Path (Join-Path $env:HOME ".cache") "ms-playwright"
            }
            # Also check runner home on GitHub Actions
            if ($env:RUNNER_TEMP) {
                $runnerHome = Split-Path -Parent $env:RUNNER_TEMP
                $playwrightCachePaths += Join-Path (Join-Path $runnerHome ".cache") "ms-playwright"
            }
        }
        
        foreach ($cachePath in $playwrightCachePaths) {
            if ((Test-Path $cachePath) -and (Get-ChildItem -Path $cachePath -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "chromium*" })) {
                $playwrightAvailable = $true
                Write-Host "  Playwright browsers found in cache: $cachePath" -ForegroundColor Gray
                break
            }
        }
    }
    
    # Final check - try to run playwright to see if it works
    if (-not $playwrightAvailable) {
        try {
            $testResult = & npx playwright --version 2>&1
            if ($LASTEXITCODE -eq 0) {
                $playwrightAvailable = $true
                Write-Host "  Playwright available via npx: $testResult" -ForegroundColor Gray
            }
        }
        catch { }
    }
}

if (-not $playwrightAvailable) {
    Write-Host "  Playwright not installed - generating HTML templates only" -ForegroundColor Yellow
    Write-Host "  To generate PNG images, run:" -ForegroundColor Yellow
    Write-Host "    npm install -D playwright" -ForegroundColor Yellow
    Write-Host "    npx playwright install chromium" -ForegroundColor Yellow
}

# Generate HTML templates (always) and PNGs (if Playwright available)
$generated = 0
$skipped = 0
$failed = 0

# Determine if we can use ProcessStartInfo or need PowerShell invocation
# On Windows with .cmd/.exe - use ProcessStartInfo
# On Linux/macOS or with npx command - use PowerShell & operator
$useProcessStartInfo = $runningOnWindows -and $npxCmd -and ($npxCmd -match '\.(exe|cmd)$')

foreach ($page in $pages) {
    $outputFile = Join-Path $OutputPath $page.filename
    
    # Skip if image already exists and not forcing
    if (-not $Force -and (Test-Path $outputFile)) {
        $skipped++
        continue
    }
    
    # Strip markdown from description and escape HTML
    $cleanDescription = Strip-Markdown $page.description
    
    # Create HTML from template
    $html = $htmlTemplate `
        -replace "{{TYPE}}", (Escape-Html $page.type) `
        -replace "{{TITLE}}", (Escape-Html $page.title) `
        -replace "{{DESCRIPTION}}", (Escape-Html $cleanDescription) `
        -replace "{{SITE_NAME}}", (Escape-Html $siteName) `
        -replace "{{URL}}", (Escape-Html $page.url) `
        -replace "{{ACCENT_COLOR}}", $accentColor `
        -replace "{{BG_COLOR}}", $bgColor `
        -replace "{{BG_COLOR_DARK}}", $bgColorDark
    
    $htmlFile = Join-Path $tempDir "$($page.slug).html"
    $html | Out-File -FilePath $htmlFile -Encoding UTF8
    
    if ($playwrightAvailable) {
        try {
            $playwrightArgs = @("--no-install", "playwright", "screenshot", "--viewport-size=1200,630", "file://$htmlFile", $outputFile)
            
            if ($useProcessStartInfo) {
                # Use ProcessStartInfo for .exe/.cmd files
                $pinfo = New-Object System.Diagnostics.ProcessStartInfo
                $pinfo.FileName = $npxCmd
                $pinfo.Arguments = $playwrightArgs -join " "
                $pinfo.RedirectStandardError = $true
                $pinfo.RedirectStandardOutput = $true
                $pinfo.UseShellExecute = $false
                $pinfo.CreateNoWindow = $true
                
                $p = New-Object System.Diagnostics.Process
                $p.StartInfo = $pinfo
                $p.Start() | Out-Null
                $p.WaitForExit(30000) | Out-Null  # 30 second timeout
                
                if ($p.ExitCode -eq 0 -and (Test-Path $outputFile)) {
                    $generated++
                    Write-Host "    Generated: $($page.filename)" -ForegroundColor Gray
                }
                else {
                    $stderr = $p.StandardError.ReadToEnd()
                    $failed++
                    Write-Host "    Warning: Failed to generate $($page.filename)" -ForegroundColor Yellow
                    if ($stderr) {
                        Write-Host "      Error: $stderr" -ForegroundColor DarkYellow
                    }
                }
            }
            else {
                # Use PowerShell invocation for npx (handles .ps1 wrappers)
                $output = & npx @playwrightArgs 2>&1
                
                if ($LASTEXITCODE -eq 0 -and (Test-Path $outputFile)) {
                    $generated++
                    Write-Host "    Generated: $($page.filename)" -ForegroundColor Gray
                }
                else {
                    $failed++
                    Write-Host "    Warning: Failed to generate $($page.filename)" -ForegroundColor Yellow
                    if ($output) {
                        Write-Host "      Error: $output" -ForegroundColor DarkYellow
                    }
                }
            }
        }
        catch {
            $failed++
            Write-Host "    Warning: Exception generating $($page.filename): $_" -ForegroundColor Yellow
        }
    }
}

# Save HTML templates for manual conversion if Playwright not available or if some failed
if (-not $playwrightAvailable -or $failed -gt 0) {
    $htmlOutputDir = Join-Path $OutputPath "html-templates"
    if (-not (Test-Path $htmlOutputDir)) {
        New-Item -ItemType Directory -Path $htmlOutputDir -Force | Out-Null
    }
    Copy-Item -Path "$tempDir\*.html" -Destination $htmlOutputDir -Force
    Write-Host "  HTML templates saved to: $htmlOutputDir" -ForegroundColor Gray
}

# Cleanup temp directory
Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
if ($playwrightAvailable) {
    Write-Host "  Generated: $generated images" -ForegroundColor Green
    if ($failed -gt 0) {
        Write-Host "  Failed: $failed images (HTML templates saved)" -ForegroundColor Yellow
    }
}
Write-Host "  Skipped: $skipped (already exist)" -ForegroundColor Gray
Write-Host "  Output: $OutputPath"

# Exit gracefully - OG images are optional, don't fail the build
exit 0
