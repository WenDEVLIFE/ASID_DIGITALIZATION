# ============================================================
# ASID Edge - Build & Package Script
# ============================================================
# Usage: .\build.ps1
# Output: ASID-Edge-v{VERSION}.zip
# ============================================================

param(
    [string]$Version = "1.1"
)

$ProjectPath = "ASID.Edge\ASID.Edge.csproj"
$PublishDir = "ASID.Edge\publish"
$ZipName = "ASID-Edge-v$Version.zip"
$Runtimes = @("win-x64")

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  ASID Edge Build & Package" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# 1. Clean previous publish output
Write-Host "`n[1/4] Cleaning previous publish..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
    Write-Host "  ✓ Removed old publish folder" -ForegroundColor Green
}

# 2. Publish the project
Write-Host "`n[2/4] Publishing project..." -ForegroundColor Yellow
foreach ($runtime in $Runtimes) {
    Write-Host "  Publishing for $runtime..." -ForegroundColor Gray
    dotnet publish $ProjectPath `
        -c Release `
        -r $runtime `
        --self-contained true `
        -p:PublishSingleFile=false `
        -o $PublishDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Build failed!" -ForegroundColor Red
        exit 1
    }
}
Write-Host "  ✓ Publish completed" -ForegroundColor Green

# 3. Verify .exe exists
$ExePath = Join-Path $PublishDir "ASID.Edge.exe"
if (-not (Test-Path $ExePath)) {
    Write-Host "`n  ✗ ASID.Edge.exe not found in publish folder!" -ForegroundColor Red
    exit 1
}
$ExeSize = (Get-Item $ExePath).Length / 1MB
Write-Host "  ✓ Found ASID.Edge.exe ($([math]::Round($ExeSize, 1)) MB)" -ForegroundColor Green

# 4. Create zip package
Write-Host "`n[3/4] Creating zip package..." -ForegroundColor Yellow
if (Test-Path $ZipName) {
    Remove-Item -Force $ZipName
}
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipName -Force

if (-not (Test-Path $ZipName)) {
    Write-Host "  ✗ Failed to create zip!" -ForegroundColor Red
    exit 1
}

$ZipSize = (Get-Item $ZipName).Length / 1MB
Write-Host "  ✓ Created $ZipName ($([math]::Round($ZipSize, 1)) MB)" -ForegroundColor Green

# 5. Summary
Write-Host "`n[4/4] Build complete!" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Output: $ZipName" -ForegroundColor Green
Write-Host "  Size:   $([math]::Round($ZipSize, 1)) MB" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
