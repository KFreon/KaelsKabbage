---
title: "Revisiting Av1"
date: 2020-09-10T17:24:44+10:00
draft: false
type: "post"
slug: "revisiting-av1"
tags: ["ffmpeg"]
---

AV1 on FFMpeg isn't so bad now!

<!--more-->  

The [announcement](https://www.zdnet.com/article/chrome-and-firefox-are-getting-support-for-the-new-avif-image-format/) of AVIF support coming to browsers got me thinking about this again.  
A recent animation I did was 14mb x264 and 12mb VP9, and that's just unacceptable, so I dove back into video encoding again!  

My VP9 settings weren't great, hence the poor compression ratio (crf was way too high quality), but I also wanted to see what AV1 did.  
FFMpeg must have done some major upgrades since 4.2.1 to 4.3.1 because it's not so abysmally slow now.  
I swear it's not...maybe it's subjective after all these 12h renders, but the savings are so huge, why wouldn't I sacrifice a few hours for 10's of % changes?  

# Setup  
- ffmpeg 4.3.1  
- 500 frames at 1920x1080  
- CRF encoding  
- x264 vs VP9 vs AV1 encoding speed  

Just note, this is basically perceptual vs speed. I'm looking at it and thinking "yeah that's about the right quality" vs the speed of encoding.  
Hardly a good set of metrics, but it's suitable for what I want.  
A significant point about this however, is the settings aren't going to be nearly the same.  
The point is to get the fastest goodest looking result.  

# Results  
| Codec | CRF | Size (mb)  | Encode Time (mins) |
| ----- | --- |  --------- | ------------------ |
| x264  |  25 |     7.1    |       .2           |
| VP9   |  40 |     8.1    |       2            |
| VP9   |  50 |     3.8    |       1            |
| AV1   |  50 |     3.8    |       14           |  

The VP9 at CRF = 50 is probably fine, but I did one with better quality anyway.  

Regardless, this is about the times, and specifically the AV1 times.  
It's better now!  
Is it good enough? In some cases definitely, in all others probably.  