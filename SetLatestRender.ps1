$folder = "./content/Renders/img"
$temp = "./static/img/temp.png"
$destination = "./static/img/recent-render.png"

Get-ChildItem -Filter "*.png" $folder | Sort-Object -Descending -Top 1 -Property LastWriteTime | Copy-Item -Destination $temp
Start-Process ffmpeg "-i $temp -y -vf scale=300:-1 $destination" -Wait
Remove-Item $temp