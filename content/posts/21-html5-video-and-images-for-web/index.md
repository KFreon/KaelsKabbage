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

I'm looking at codecs, not conatiners. This is better explained by others: 
LINKS TOP BETTER POSTS  

Essentially, MP4/MKV are containers, boxes that contain codecs (mp3, h264, VP9).

# The Setup
Moving forward, the codecs I care about are:  

### Video
- AV1
- H264
- VP9

I'm not including H265 since it's not royalty free and probably doesn't have much future against these free ones.  

### Image
- webp (VP8 I think?)
- png
- jpg

## Tools
[FFMpeg](https://www.ffmpeg.org/) and the [Webp](https://developers.google.com/speed/webp) tools from google because they're common, simple, and reasonably well documented.  

## Sources
The sources I'm using are pngs from a Blender animation: ~8mb each, 1100 of them.  

# The Plan
I want to provide the best tradeoff between size and quality to users, while still providing compatibilty to those without the shiny toys (often mobile users).  
This involves having a widely supported codec available, but providing better ones to those who can use them.  
It's encoding time!  

# Images
Generally speaking, png's are a lot of unnecessary bytes to transfer, and while jpg is less bytes, it suffers visually.  
The only one I really considered was Webp as AVIF isn't supported yet and webp is widely supported.    

## Webp  
[Webp](https://developers.google.com/speed/webp) is a Google driven format based on their VP8 (VP9 now?) video compression, just being applied to a single image.  
It's USUALLY a fair bit smaller than a jpg with better quality.  
I say usually because sometimes it can be higher, but generally it's lower. 

{{< image path="img/WebpComparison" alt="Comparison of webp compared to png and jpg" >}}  

The original images were two of my renders, and as the above comparison shows, webp is smaller and that size difference can vary a LOT.  
Quality-wise, they're almost indistinguishable (you can check by viewing them in the [renders]() page - clicking them gives the png version)

### Command Line  
`cwebp sword.png -o sword.webp -mt -m 6 -pass 10 -q 90`  

- mt = multithreading
- m 6 = quality/compression speed. 0-6, higher is slower but smaller.
- pass 10 = Higher is more time spent looking for quality
- q 90 = quality 0-100, higher is better

# Video  
Quick breakdown:  

- [h264](https://trac.ffmpeg.org/wiki/Encode/H.264)
    - Very common and widespread  
    - Decent balance between encoding speed, filesize, and quality
- [VP9](https://trac.ffmpeg.org/wiki/Encode/VP9)
    - Slower encoding time
    - USUALLY smaller and better quality than h264
- [AV1](https://trac.ffmpeg.org/wiki/Encode/AV1)
    - Slow encoding...I mean SLOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOW (like 500x slower than h264)
    - USUALLY smaller and better quality than VP9

There's more interesting tradeoffs for video, since you don't want to sit around until the heat death of the universe waiting for that perfect AV1 encode.  
AV1 is really slow to encode right now. VP9 is faster, but does suffer jpg-like blocking artifacts at lower bitrates.  

> AV1 12 bit colour doesn't seem to work in browsers. The stutter is real. The PNG's I had were apparently 12 bit colour (yuv444p12le), and the default for FFMpeg was to use the input pixel format. Use `-pix_fmt yuv444p` (or `yuv420p`/`yuv422p`) to workaround.  

SIZE COMPARISON
size, time taken

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

