param(
[string]$output,
[switch]$isVideo = $False,
[string]$audio = "",
[string]$fps = 25
)


if ($isVideo) {
	Write-Host
	Write-Host "**Main video**"
	FFmpegConverter.ps1 -crf 25 -fps $fps -audio $audio -output $output
	
	Write-Host
	Write-Host "**AV1**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -av1

	Write-Host
	Write-Host "**AV1 Halfsize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -av1 -halfsize

	Write-Host
	Write-Host "**AV1 Quartersize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -av1 -smaller

	Write-Host
	Write-Host "**Webm**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -vp9

	Write-Host
	Write-Host "**Webm Halfsize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -vp9 -halfsize

	Write-Host
	Write-Host "**Webm Quartersize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -audio $audio -output $output -vp9 -smaller
}
else{
	Write-Host("Did you mean '-isVideo'? Can't remember why I needed it")
}