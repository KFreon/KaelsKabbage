---
title: "Performance: Caching MediatR Queries"
date: 2021-07-02T08:38:33+10:00
draft: true
type: "post"
slug: "mediatr-caching"
tags: ['mediatr', 'caching']
---

TODO
- fix sizing on small screen
- fix hugo :(



[Previously]({{< ref "/posts/26-azure-perf-investigation" >}}), I did some performance investigations in Azure. The cause remains elusive, so we've started looking into caching, most interestingly caching Mediatr Query results.  

<!--more-->  

At `client`, we make extensive use of [Mediatr](https://github.com/jbogard/MediatR) queries/commands for read/write separation. We have dropdowns populated by query, queries that call queries, etc.  
While this makes our code a bit easier to understand due to all the reuse, it does make things less performant.  
We could have written something more specific and utilising `joins` and whatnot to reduce database calls, BUT right now, we still prefer readability and maintainability over raw performance.  
BUT this is starting to get us and `client` down, so we've been exploring our options.  


# Caching  
[Redis](https://redis.io/topics/introduction) came up during our discussion, and it is likely to be the solution in time, but it's also more work than we have time to implement right now.  
We currently have some [ResponseCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-5.0) using `ETAGs`, but it's hard to use those everywhere, especially since the [docs](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-5.0#responsecache-attribute) recommend against it's use for authenticated requests.  
Since we fetch dropdowns and that kind of thing, we considered caching the responses of their controllers, essentially `ResponseCache` but manually...  
In the end, we realised that the queries we used in the controllers were often reused in other queries, as the dropdown info was what we were storing on our Aggregates anyway.  
So if we reuse the mediatr queries so much, why don't we cache the results?  

# Mediatr Caching  
The initial challenges were simple enough, there's a couple of libraries that already do this, however they didn't quite do what I wanted.  
[This one](https://github.com/SorenZ/Alamut.MediatR.Caching) by SorenZ and [this one](https://github.com/Imprise/Imprise.MediatR.Extensions.Caching) by Imprise was pretty close, but didn't give us good invalidation (or something...I can't remember now).  
Let's take a quick look at the eventual solution.  

## Registering cache  


## Cache Invalidation  
Cache Invalidation is one of those hard problems :D  
We've got several invalidation mechanisms supported:  

- Sliding expiration  
- Absolute expiration  
- DI registered hooks (command/write triggers)  
- Manual invalidation via DI injection  



## Improvements?  
So, did it help?  
Well, below is a comparison of the same duration before and after the change to the most common Mediatr query.  
{{< image path="img/MediatorCacheDurationComparison_OLD" alt="5 days of data BEFORE caching" >}}  
{{< image path="img/MediatorCacheDurationComparison_NEW" alt="5 days of data AFTER caching" >}}  

For those who like tabular info:  

|       test       | Before | After | Ratio |  
|--------------|--------|-------|-------|  
| Count        | 197k   | 305k  | 1.55x |  
| Duration (s) | 1,563  | 392   | 0.25x |  

Even with 50% more queries, the SUM duration is 75% shorter!  

WebApplication1 in source/repos
[Full example solution]()