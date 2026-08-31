<#
.SYNOPSIS
    Resets the ASID Edge local data (database + session).

.DESCRIPTION
    Offers multiple reset options:
    1. Clear session only (fixes login issues after logout)
    2. Reset database only (clears transactions, lanes)
    3. Full reset (both session + database)

    Run this while the app is CLOSED for best results.

.NOTES
    Session: %LOCALAPPDATA%\ASID\session.json
    Database: %LOCALAPPDATA%\ASID\asid_local.db
#>

$ErrorActionPreference = "Stop"

$dbDir      = Join-Path $env:LOCALAPPDATA "ASID"
$dbPath     = Join-Path $dbDir "asid_local.db"
$sessionPath = Join-Path $dbDir "session.json"

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

# ── Show menu ──
Write-Host ""
Write-Host "  =========================================" -ForegroundColor Cyan
Write-Host "    ASID Edge - Local Data Reset" -ForegroundColor Cyan
Write-Host "  =========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  [1] Clear session only" -ForegroundColor White
Write-Host "      Fixes: can't login after logout" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  [2] Reset database only" -ForegroundColor White
Write-Host "      Clears: transactions, lanes" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  [3] Full reset (session + database)" -ForegroundColor White
Write-Host "      Clears: everything, start fresh" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  [Q] Quit" -ForegroundColor DarkGray
Write-Host ""

$choice = Read-Host "  Select option (1/2/3/Q)"

switch ($choice) {
    "1" {
        # ── Clear session only ──
        Write-Host ""
        if (Test-Path $sessionPath) {
            $info = Get-Item $sessionPath
            Write-Host "  Session file: $sessionPath" -ForegroundColor Cyan
            Write-Host "  Size: $([math]::Round($info.Length / 1KB, 1)) KB" -ForegroundColor White
            Write-Host ""
            $confirm = Read-Host "  Delete session file? (y/N)"
            if ($confirm -notin @("y", "Y", "yes", "YES")) {
                Write-Host "  Aborted." -ForegroundColor Red
                exit 1
            }
            try {
                Remove-Item -Path $sessionPath -Force
                Write-Host ""
                Write-Host "  Session cleared! You can now login again." -ForegroundColor Green
                Write-Host ""
            } catch {
                Write-Host "  Failed to delete: $_" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "  No session file found. Nothing to clear." -ForegroundColor Green
            Write-Host ""
        }
    }
    "2" {
        # ── Reset database only ──
        Write-Host ""
        if (-not (Test-Path $dbPath)) {
            Write-Host "  Database not found at:" -ForegroundColor Cyan
            Write-Host "  $dbPath" -ForegroundColor White
            Write-Host "  Nothing to delete. App will create fresh DB on next launch." -ForegroundColor Green
            exit 0
        }
        $info = Get-Item $dbPath
        Write-Host "  Database file: $dbPath" -ForegroundColor Cyan
        Write-Host "  Size: $([math]::Round($info.Length / 1KB, 1)) KB" -ForegroundColor White
        Write-Host ""
        Write-Host "  This will DELETE all local data (transactions, lanes)." -ForegroundColor Yellow
        $confirm = Read-Host "  Are you sure? (y/N)"
        if ($confirm -notin @("y", "Y", "yes", "YES")) {
            Write-Host "  Aborted." -ForegroundColor Red
            exit 1
        }
        try {
            Remove-Item -Path $dbPath -Force
            Write-Host ""
            Write-Host "  Database deleted! App will recreate on next launch." -ForegroundColor Green
            Write-Host ""
        } catch {
            Write-Host "  Failed to delete: $_" -ForegroundColor Red
            exit 1
        }
    }
    "3" {
        # ── Full reset ──
        Write-Host ""
        Write-Host "  This will DELETE everything:" -ForegroundColor Yellow
        Write-Host "  - Session file (auto-login)" -ForegroundColor Yellow
        Write-Host "  - Database (transactions, lanes)" -ForegroundColor Yellow
        Write-Host ""
        $confirm = Read-Host "  Are you sure? (y/N)"
        if ($confirm -notin @("y", "Y", "yes", "YES")) {
            Write-Host "  Aborted." -ForegroundColor Red
            exit 1
        }
        try {
            $deleted = 0
            if (Test-Path $sessionPath) {
                Remove-Item -Path $sessionPath -Force
                Write-Host "  [OK] Session file deleted" -ForegroundColor Green
                $deleted++
            }
            if (Test-Path $dbPath) {
                Remove-Item -Path $dbPath -Force
                Write-Host "  [OK] Database deleted" -ForegroundColor Green
                $deleted++
            }
            if ($deleted -eq 0) {
                Write-Host "  Nothing to delete. Already clean." -ForegroundColor Green
            } else {
                Write-Host ""
                Write-Host "  Full reset complete! Open ASID Edge to start fresh." -ForegroundColor Green
            }
            Write-Host ""
        } catch {
            Write-Host "  Failed: $_" -ForegroundColor Red
            exit 1
        }
    }
    default {
        Write-Host "  Cancelled." -ForegroundColor DarkGray
        exit 0
    }
}
