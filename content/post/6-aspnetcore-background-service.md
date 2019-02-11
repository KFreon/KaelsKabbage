---
title: "ASP.NET Core Background Service"
date: 2018-08-27T09:15:29+10:00
draft: false
type: "post"
slug: "aspnetcore-background-service"
---

Background tasks are often delegated to worker projects or external libraries to be handled, but ASP.NET Core has something like this inbuilt.  
The actual interface is {{< inline "IHostedService" >}} and is registered in {{< inline "startup.cs" >}} like:  
```csharp 
services.AddHostedServices<SOME_BACKGROUND_SERVICE>
```
That's cool, but there's also a built in abstract class to handle a lot of the plumbing as well called {{< inline "BackgroundService" >}}.  
Implementing this class requires an override for {{< inline "ExecuteAsync(CancellationToken stoppingToken)" >}} and that's it. I'm looking to use this for doing simple repeating tasks without extra libraries.   