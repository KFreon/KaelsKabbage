param([Boolean]$forceRebuild=$False)

$images = Get-ChildItem "./static/img" -Filter "*.png" -File -Recurse

$images | ForEach-Object {
    $fileExists = Test-Path $_.FullName
    if (!$forceRebuild -and $fileExists)
    {
        continue
    }

    Write-Host "Converting " $_.Name "..."
    $newPath = Join-Path $_.DirectoryName -ChildPath "$($_.BaseName).webp"
    Start-Process "C:\Users\kaell\Documents\Tools\Webp\bin\cwebp.exe" "-q 80 $($_.FullName) -o $newPath"
}