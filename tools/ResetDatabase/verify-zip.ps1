Add-Type -Assembly 'System.IO.Compression.FileSystem'
$zipPath = Join-Path $PSScriptRoot '..\..\ASID-Edge-v1.0.zip'
$entries = [System.IO.Compression.ZipFile]::OpenRead($zipPath).Entries
foreach ($e in $entries) {
    if ($e.FullName -like '*Reset*') {
        Write-Host "$($e.FullName)  ($($e.Length) bytes)"
    }
}
