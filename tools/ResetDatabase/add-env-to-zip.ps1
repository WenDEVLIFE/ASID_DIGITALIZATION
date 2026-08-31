Add-Type -Assembly 'System.IO.Compression.FileSystem'

$zipPath = Join-Path $PSScriptRoot '..\..\ASID-Edge-v1.0.zip'
$envPath = Join-Path $PSScriptRoot '..\..\.env'

if (-not (Test-Path $envPath)) {
    Write-Host ".env file not found at: $envPath" -ForegroundColor Red
    exit 1
}

$zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Update')
[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $envPath, '.env') | Out-Null
$zip.Dispose()

Write-Host "Added: .env" -ForegroundColor Green
Write-Host "Done!" -ForegroundColor Green
