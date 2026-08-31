Add-Type -Assembly 'System.IO.Compression.FileSystem'

$zipPath = Join-Path $PSScriptRoot '..\..\ASID-Edge-v1.0.zip'
$zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Update')

$files = @(
    @{ Src = 'Reset-Database.ps1'; Dest = 'ResetDatabase\Reset-Database.ps1' },
    @{ Src = 'Reset-Database.bat'; Dest = 'ResetDatabase\Reset-Database.bat' }
)

foreach ($f in $files) {
    $srcPath = Join-Path $PSScriptRoot $f.Src
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $srcPath, $f.Dest) | Out-Null
    Write-Host "Added: $($f.Dest)"
}

$zip.Dispose()
Write-Host "Done! Files added to ASID-Edge-v1.0.zip"
