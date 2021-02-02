$folder = "./content/Renders/img/*"
$destination = "./static/img/recent-render.png"

# Webm because there's definitely only one.
# mp4 could have AV1 and h264
$recentRender = Get-ChildItem -Include "*.png","*.webm" $folder | Sort-Object -Descending -Top 1 -Property LastWriteTime
$fileName = [IO.Path]::GetFileName($recentRender)
$ext = [IO.Path]::GetExtension($recentRender)

Write-Output "Setting recent render: $fileName"
if ($ext -eq '.webm') {
    Start-Process ffmpeg "-i $recentRender -y -vframes 1 -vf scale=300:-1 $destination" -Wait -RedirectStandardError $true
} else {
    Start-Process ffmpeg "-i $recentRender -y -vf scale=300:-1 $destination" -Wait -RedirectStandardError $true
}

Write-Output "Finished!"