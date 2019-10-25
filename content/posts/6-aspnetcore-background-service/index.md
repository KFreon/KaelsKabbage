---
title: "ASP.NET Core Background Service"
date: 2018-08-27T09:15:29+10:00
draft: false
type: "post"
slug: "aspnetcore-background-service"
tags: ["aspnet"]
---

Background tasks are often delegated to worker projects or external libraries to be handled, but ASP.NET Core has something like this inbuilt.  
The actual interface is `IHostedService` and is registered in `startup.cs` like:  
```csharp 
services.AddHostedServices<SOME_BACKGROUND_SERVICE>
```
<!--more-->  


That's cool, but there's also a built in abstract class to handle a lot of the plumbing as well called `BackgroundService`.  
Implementing this class requires an override for `ExecuteAsync(CancellationToken stoppingToken)` and that's it. I'm looking to use this for doing simple repeating tasks without extra libraries.   