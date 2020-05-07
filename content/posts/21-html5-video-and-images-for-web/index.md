---
title: "Html5 Video and Images for Web"
date: 2020-05-07T09:36:21+10:00
draft: true
type: "post"
slug: "html5-video-and-images-for-web"
tags: ["ffmpeg"]
---



<!--more-->  

I like being efficient with my bits and bytes. I bought into the `webp` image format train pretty early, adopting it when I made this blog.  
Recently I made a video longer than 2 seconds, so I started looking properly into codecs like VP9 and AV1.  
Lets take a look into some video and image codecs and how the new shiny ones can be used.  

Quick aside, MP4/MKV are containers, boxes that contain codecs (mp3, h264, VP9).  
Codecs are the things I'm looking at here.

# The Setup

|        |       |
| ------ | ----- |
| Video Codecs  | AV1, h264, VP9 |
| Image Codecs  | webp , png, jpg  |
| Video Sources | PNGs from a Blender animation: ~8mb each, 1100 of them. [Result]() |
| Image Sources | [The Floating Rock render PNG]() and [Sword]() |
| Tools  | [FFMpeg](https://www.ffmpeg.org/) and the [Webp](https://developers.google.com/speed/webp) tools from google because they're common, simple, and reasonably well documented. |

# The Plan
I want to provide the best tradeoff between size and quality to users, while still providing compatibilty to those without the shiny toys (often mobile users).  
This involves having a widely supported codec available, but providing better ones to those who can use them.  
It's encoding time!  

# Image Codecs
Generally speaking, png's are a lot of unnecessary bytes to transfer, and while jpg is less bytes, it suffers visually.  
The only one I really considered was Webp as AVIF isn't supported yet and webp is widely supported.    

## Webp  
[Webp](https://developers.google.com/speed/webp) is a Google driven format based on their VP8 (VP9 now?) video compression, just being applied to a single image.  
It's USUALLY a fair bit smaller than a jpg with better quality.  
I say usually because sometimes it can be larger, but generally it's smaller. 

### Details  
{{% split %}}
{{% splitLeft title="Floating Rock" %}}
| Format | Size (kb) |
| ------ | ---- |
| png    | 3599 |
| jog    | 558  |
| webp   | 311  |
{{% /splitLeft %}}
{{% splitRight title="Sword" %}}
| Format | Size (kb) |
| ------ | ---- |
| png    | 8022 |
| jog    | 209  |
| webp   | 87   |
{{% /splitRight %}}
{{% /split %}}  

### Command Line  
`cwebp sword.png -o sword.webp -mt -m 6 -pass 10 -q 90`  

- mt = multithreading
- m 6 = quality/compression speed. 0-6, higher is slower but smaller.
- pass 10 = Higher is more time spent looking for quality
- q 90 = quality 0-100, higher is better

# Video  

| Codec | Adoption | Size | Quality | Encoding Speed | Notes |
| ----- | -------- | ---- | ------- | -------------- | ----- |
| [h264](https://trac.ffmpeg.org/wiki/Encode/H.264) | [Common and widespread](https://caniuse.com/#feat=mpeg4) | Reference | Good | Good | Good balance between encoding speed, filesize, and quality |
| [VP9](https://trac.ffmpeg.org/wiki/Encode/VP9) | [Generally well supported](https://caniuse.com/#feat=webm) | Often 10x smaller | Decent | Much slower | Much smaller with limited hit to quality, but much slower encode speed.
| [AV1](https://trac.ffmpeg.org/wiki/Encode/AV1) | [Desktop, not mobile](https://caniuse.com/#feat=av1) | Often 10x smaller | Good | Sooo much slower | Smaller, better quality, but limited support and really slow encode time |

There's more interesting tradeoffs for video, since you don't want to sit around until the heat death of the universe waiting for that perfect AV1 encode.  
AV1 is really slow to encode right now, like 500x slower. VP9 is faster, but does suffer jpg-like blocking artifacts at lower bitrates.  

> AV1 10 and 12 bit colour don't seem to work in browsers. The stutter is real. The PNG's I had were apparently 12 bit colour (`yuv444p12le`), and the default for FFMpeg was to use the input pixel format. Use `-pix_fmt yuv444p` (or `yuv420p`/`yuv422p`) to workaround.  

| Codec | Size (mb) | Encode Time (mins) |
| ----- | --------- | ------------------ |
| h264  | 15.6      | 2                  |
| VP9   | 4.86      | 12                 |
| AV1   | 2.06      | 140                |

## Command lines
Note that this is slightly different from the usual commands around, since I'm using an image sequence instead of another video.  

### Common notes
- `scale` is because 1080 would have taken too long
- `-1:720` to keep aspect while reducing height to 720
    - `lanczos` scale filter for good resize quality
- `row-mt` and `tile-columns` and `threads` for multithreading (more apparently faster but slightly lower quality) 
- `-movflags +faststart` is for MP4 containers, and lets the file play while still downloading

### H264  
`ffmpeg -framerate 30 -i %04d.png -vf scale=-1:720:flags=lanczos -c:v libx264 -b:v 0 -crf 35 -movflags +faststart output.mp4`  

### VP9  
`ffmpeg -framerate 30 -i %04d.png -vf scale=-1:720:flags=lanczos -c:v libvpx-vp9 -b:v 1024k -minrate 512k -maxrate 2000k -crf 10 -deadline best -row-mt 1 -tile-columns 2 -threads 8 output.webm`  

VP9 needed some quality tweaking to get what I wanted.  

- Target bitrate mode:
    - Aim for 1024k bitrate
    - Allow bitrate to vary down to 512k and up to 2000k 
- deadline is like profile. Best means it's slower but better compression

### AV1  
`ffmpeg -framerate 30 -i %04d.png -vf scale=-1:720:flags=lanczos -c:v libaom-av1 -b:v 0 -crf 35 -strict experimental -row-mt 1 -cpu-used 5 -tile-columns 2 -threads 8 -pix_fmt yuv444p -movflags +faststart output.mp4`

> Note the `experimental` flag since AV1 is new

# Usage in the web  
HTML5 controls support these new formats by allowing the developer to provide several formats and allow the client to only pull the bytes for the first supported format.  

## HTML5 Images  
```html
<picture>
    <source srcset="path-to-image.webp" type="image/webp">
    <source srcset="path-to-image.png" type="image/png">
    <img src="path-to-image.png">
</picture>
```

The above will let the client pull the webp if it's supported and won't download the png. If webp isn't supported by the browser, it'll fallback to the png. In that case, the webp won't be downloaded at all, as the browser knows it doesn't support it.  

> That `img` tag is required. Not 100% why, but it is.  

## HTML5 Video 
```html
<video autoplay muted playsinline loop>
    <source src="path-to-AV1.mp4" type="video/mp4; codecs=av01.0.05M.08">
    <source src="path-to-VP9.webm" type="video/webm; codecs=vp9">
    <source src="path-to-H264.mp4" type="video/mp4">
    This message is displayed when none are supported
</video>
```

Similarly to the `picture` tag, this allows the browser to only download the video codec it supports in the order of declaration.  
That weird codec string is how the browser identifies the AV1 codec.  
This tag also supports those usual attributes like `autoplay` and `loop`.  

