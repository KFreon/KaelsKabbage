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

# Codecs vs Containers  
Real quick, I want to make sure we're all on the same page.  

## Containers
MP4 is not a "format" in the sense that MP4 has better compression than MKV for example.  
[MP4](https://en.wikipedia.org/wiki/MPEG-4_Part_14) is a container, inside which are video, audio, and subtitle streams, along with some other bits.  
That's the limit of my understanding of these, so let's just say: MP4/MKV are boxes to put video and audio into. 

## Codecs  
These are the standard or ruleset describing a method of encoding/decoding data.  
In the video world, [h264](https://www.divx.com/en/software/technologies/h264/) is currently the most common (anocdotal I guess) codec and is used to compress video significantly down from the older divx and such.  

## Reason I care about the distinction
Wholistically, I think of things as: Containers contain codecs.  
Containers are just boxes. I'm sure there's some reasons to choose one over the other, but it doesn't affect the quality of the stream inside (can affect seeking, etc).  
Regarding quality and filesize, you always want to be thinking about codecs.  

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

I'll be using [FFMpeg](https://www.ffmpeg.org/) and the [Webp](https://developers.google.com/speed/webp) tools from google because they're common, simple, and reasonably well documented.  

# The Plan
I want to provide the best tradeoff between size and quality to users, while still providing compatibilty to those without the shiny toys (often mobile users).  
This involves having a widely supported codec available, but providing better ones to those who can use them.  
It's encoding time!  

# Images
The two base image types are png and jpg.  
Generally speaking:
- png is:
    - Lossless (No image data is destroyed)
    - Higher quality
    - Larger on disk
- jpg is:
    - Lossy (Image data is destroyed to save space)
    - Lower quality
    - Significantly smaller (usually)

With that in mind, png's are a lot of unnecessary bytes to transfer, and while jpg is less, it suffers visually.  
What options do we have?  

## Webp  
[Webp](https://developers.google.com/speed/webp) is a Google driven format based on their VP8 (VP9 now?) video compression, just being applied to a single image.  
It's USUALLY a fair bit smaller than a jpg with better quality.  

SIZE COMPARISON
CONVERT STRINGS

## AVIF  
MAYBE?  


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

Some things to note here, AV1 is really slow to encode right now. VP9 is faster, but does suffer jpg-like blocking artifacts at lower bitrates.  

> MAJOR NOTE: AV1 12 bit colour doesn't seem to work in browsers. The stutter is real. The PNG's I had were apparently 12 bit colour (yuv444p12le), and the default for FFMpeg was to use the input pixel format. Use `-pix_fmt yuv444p` (or yuv420p/yuv422p) to workaround.  

SIZE COMPARISON
CONVERT STRINGS

# Usage in the web  
HTML5 controls support these new formats by allowing the developer to provide several formats and allow the client to only pull the bytes for the first supported format.  

## HTML5 Images  
IMAGE TAG

## HTML5 Video 
VIDEO TAG