<#
.SYNOPSIS
    Resets the ASID Edge local SQLite database.

.DESCRIPTION
    Deletes the local SQLite database file so the app recreates it fresh
    on next launch. Run this while the app is CLOSED for best results.

.NOTES
    Database location: %LOCALAPPDATA%\ASID\asid_local.db
#>

$ErrorActionPreference = "Stop"

$dbDir  = Join-Path $env:LOCALAPPDATA "ASID"
$dbPath = Join-Path $dbDir "asid_local.db"

# ── Check if the app is running ──
$procs = Get-Process -Name "ASID.Edge*" -ErrorAction SilentlyContinue
if ($procs) {
    Write-Host ""
    Write-Host "  WARNING: ASID Edge is currently running!" -ForegroundColor Yellow
    Write-Host "  Close the app first to avoid file-lock errors." -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "  Continue anyway? (y/N)"
    if ($continue -notin @("y", "Y", "yes", "YES")) {
        Write-Host "  Aborted." -ForegroundColor Red
        exit 1
    }
}

# ── Check if the database file exists ──
if (-not (Test-Path $dbPath)) {
    Write-Host ""
    Write-Host "  Database file not found at:" -ForegroundColor Cyan
    Write-Host "  $dbPath" -ForegroundColor White
    Write-Host ""
    Write-Host "  Nothing to delete. The app will create a fresh database on next launch." -ForegroundColor Green
    exit 0
}

# ── Show file info ──
$fileInfo = Get-Item $dbPath
$sizeKB   = [math]::Round($fileInfo.Length / 1KB, 1)
$modified = $fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")

Write-Host ""
Write-Host "  Database file:" -ForegroundColor Cyan
Write-Host "  Path:     $dbPath"
Write-Host "  Size:     $sizeKB KB"
Write-Host "  Modified: $modified"
Write-Host ""

# ── Confirm ──
Write-Host "  This will DELETE all local data (transactions, lanes)." -ForegroundColor Yellow
Write-Host "  The app will recreate the database on next launch." -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "  Are you sure? (y/N)"
if ($confirm -notin @("y", "Y", "yes", "YES")) {
    Write-Host "  Aborted." -ForegroundColor Red
    exit 1
}

# ── Delete ──
try {
    Remove-Item -Path $dbPath -Force
    Write-Host ""
    Write-Host "  Database deleted successfully!" -ForegroundColor Green
    Write-Host "  Open ASID Edge to start fresh." -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "  Failed to delete database:" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Make sure the app is fully closed and try again." -ForegroundColor Yellow
    exit 1
}
