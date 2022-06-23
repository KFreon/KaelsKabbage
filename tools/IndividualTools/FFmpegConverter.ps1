param(
  [string]$crf=40,
  [switch]$halfsize=$False,
  [switch]$smaller=$False,
  [switch]$video=$False,
  [string]$videoSrc="",
  [switch]$vp9=$False,
  [switch]$av1=$False,
  [string]$fps=30,
  [string]$output="Output",
  [string]$bv=0,
  [string]$pixfmt="yuv444p",
  [string]$extras="",
  [switch]$help=$False,
  [string]$start=0,
  [string]$digits=4,
  [string]$prefix='',
  [switch]$webp=$False)

if ($help) {
	Write-Host "***FFmpegConverter***" -ForegroundColor Green
	Write-Host "Use in folder with png's named dddd.png"
	Write-Host "Arguments:" -ForegroundColor Red
	Write-Host "    -crf: Quality, lower is higher quality and size.             " $crf -ForegroundColor Yellow
	Write-Host "    -halfsize: If provided, renders at half the input size.      " $halfsize -ForegroundColor Yellow
	Write-Host "    -smaller: If provided, renders at 250px.                     " $smaller -ForegroundColor Yellow
	Write-Host "    -video: Specifies video input.                               " $video -ForegroundColor Yellow
	Write-Host "    -videoSrc: If provided, source of video.                     " $videoSrc -ForegroundColor Yellow
	Write-Host "    -vp9: Outputs a VP9 webm.                                    " $vp9 -ForegroundColor Yellow
	Write-Host "    -av1: Outputs an AV1 MP4.                                    " $av1 -ForegroundColor Yellow
	Write-Host "    -fps: Framerate of output.                                   " $fps -ForegroundColor Yellow
	Write-Host "    -output: Name of generated file.                             " $output -ForegroundColor Yellow
	Write-Host "    -bv: Used for other quality type (Can't remember now...)     " $bv -ForegroundColor Yellow
	Write-Host "    -pixfmt: Pixel format for output. Usually required for VP9.  " $pixfmt -ForegroundColor Yellow
	Write-Host "    -extras: Any other ffmpeg cmdline arguments.                 " $extras -ForegroundColor Yellow
  Write-Host "    -start: Start frame number.                                  " $start -ForegroundColor Yellow
  Write-Host "    -digits: Number of digits in the filename.                   " $digits -ForegroundColor Yellow
  Write-Host "    -prefix: Filename prefix.                                    " $prefix -ForegroundColor Yellow
  Write-Host "    -webp: Use webps.                                            " $webp -ForegroundColor Yellow
	Exit
}

$encoder = if ($vp9 -or $video) {"libvpx-vp9"} elseif ($av1) {"libsvtav1"} else {"libx264"}
$outputName = if ($vp9) {"${output}_VP9.webm"} elseif ($av1) {"${output}_AV1.mp4"} elseif($video) {"${output}_halfsize.mp4"} else {"$output.mp4"}
$scaleExpression = if ($halfsize) {"-vf scale=-1:720:flags=lanczos"} elseif($smaller) {"-vf scale=-1:360:flags=lanczos"}
$start = if ($start -ne 0) {"-start_number $start"} else {""}
$ext = if ($webp) {"webp"} else {"png"}


if ($video) 
{
  Start-Process ffmpeg "-i $videoSrc $scaleExpression -enable-tpl 1 -c:v $encoder -b:v $bv -crf $crf -row-mt 1 -threads 8 -pass 1 -pix_fmt $pixfmt $extras -f null -" -Wait -NoNewWindow

  Start-Process ffmpeg "-i $videoSrc $scaleExpression -tiles 2x2 -enable-tpl 1 -c:v $encoder -b:v $bv -crf $crf -row-mt 1 -threads 8 -speed 2 -pass 2 -pix_fmt $pixfmt $extras $outputName" -Wait -NoNewWindow
} 
elseif ($vp9) 
{
  Start-Process ffmpeg "-framerate $fps $start -i ${prefix}%0${digits}d.${ext} $scaleExpression -enable-tpl 1 -c:v $encoder -b:v $bv -crf $crf -row-mt 1 -threads 8 -pass 1 -pix_fmt $pixfmt $extras -f null -" -Wait -NoNewWindow

  Start-Process ffmpeg "-framerate $fps $start -i ${prefix}%0${digits}d.${ext} $scaleExpression -tiles 2x2 -enable-tpl 1 -c:v $encoder -b:v $bv -crf $crf -row-mt 1 -threads 8 -speed 2 -pass 2 -pix_fmt $pixfmt $extras $outputName" -Wait -NoNewWindow
} 
elseif ($av1) 
{
  Start-Process ffmpeg "-framerate $fps $start -i ${prefix}%0${digits}d.${ext} $scaleExpression -c:v $encoder -qp $crf -preset 3 $extras $outputName" -Wait -NoNewWindow
} 
else 
{
  Start-Process ffmpeg "-framerate $fps $start -i ${prefix}%0${digits}d.${ext} $scaleExpression -c:v $encoder -b:v $bv -crf $crf -pix_fmt $pixfmt -threads 8 $extras $outputName" -Wait -NoNewWindow
}