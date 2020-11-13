---
title: "Playing with Blazor in .NET 5"
date: 2020-11-13T14:39:27+10:00
draft: true
type: "post"
slug: "blazor-net5"
tags: ["blazor"]
---

[.NET 5](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) was released recently, with boatloads of goodies.  
Blazor got some attention in this release, and I had some free time to play with it.  
There's good news and bad news...

<!--more-->  

# TL;DR  

# What this post is and isn't  
This isn't to detail how to Blazor, or compare between Server and Webassembly implementations, or performance.  
It's just about File --> New Project and spinning up some basic Use Cases and how that felt.  

# Setup  
- VS 16.8  
- .NET 5 day 1 (no minor versions)  
- Blazor Server (NOT Webassembly, the Signalr update one)  

# File --> New Project: BLAZOR  
The initial setup experience is fairly nice.  
Projects are scaffolded out with useful yet minimal setup, e.g. Some basic components as a reminder of how to do things, Bootstrap included to reduce requirements on custom styling.  

# Not-so-hot reload  
