param(
[string]$output,
[switch]$isVideo = $False,
[string]$fps = 25
)

if ($isVideo) {
	Write-Host "**Main video**"
	FFmpegConverter.ps1 -crf 25 -fps $fps -output $output
	
	Write-Host "**AV1**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -av1

	Write-Host "**AV1 Halfsize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -av1 -halfsize

	Write-Host "**AV1 Quartersize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -av1 -smaller

	Write-Host "**Webm**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -vp9

	Write-Host "**Webm Halfsize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -vp9 -halfsize

	Write-Host "**Webm Quartersize**"
	FFmpegConverter.ps1 -crf 40 -fps $fps -output $output -vp9 -smaller
}
else{

}