param([Boolean]$forceRebuild=$False)

$images = Get-ChildItem "./static/img" -Filter "*.png" -File -Recurse
$images | ForEach-Object {
    $newPath = Join-Path $_.DirectoryName -ChildPath "$($_.BaseName).webp"
    $fileExists = Test-Path $newPath
    
    if (!(!$forceRebuild -and $fileExists))
    {
        Write-Host "Converting " $_.Name "..."
        Start-Process "C:\Users\kaell\Documents\Tools\Webp\bin\cwebp.exe" "$($_.FullName) -o $newPath"
    }    
}