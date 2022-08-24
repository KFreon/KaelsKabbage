---
title: "Android Emulator: Connecting to Localhost"
date: 2022-06-03T12:59:47+10:00
type: "post"
slug: "android-emulator-localhost"
tags: ["android"]
---

I often use the Android Emulator to do testing on a Xamarin Forms app, or Cordova, or something of the sort.  
It's a long round trip to point at test when you want to debug the backend behaviour, so let's point it at local dev!

<!--more-->  

The Android Emulator maps `10.0.2.2` to `localhost`, so can we just do that?
In Kestral, yes!  
I changed my Xamarin Forms app to point at `http://10.0.2.2:5042` (my Kestral backend) and it Just Works.  

Wonderful, but...what if we're working with IIS?

## IIS = Intrinsically Irritating Server
Here's the process I found after much Stack overflow (but essentially [this one](https://stackoverflow.com/questions/6192726/android-emulator-loopback-to-iis-express-does-not-work-but-does-work-with-cassi))

- Close VS
- Getting Invalid Hostname? Need to edit: `\.vs\<project-name>\config\ApplicationHost.config`
- Find `<bindings protocal="http"` line
- Copy the Http line and replace `localhost` with `*`
  > Request from emulator isn't coming from localhost, it's coming from 10.0.2.2 or some redirection, so need to allow it in IIS (since it's first in the chain)  
  > EDIT: May not need to copy, might be able to just set it to `":<port>:"` (no stars)
- Open VS AS ADMIN (or you'll get "can't connect to IIS server")
- Confirm by visiting that address in emulator browser