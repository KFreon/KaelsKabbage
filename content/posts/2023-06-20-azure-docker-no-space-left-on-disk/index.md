---
title: "Azure Docker: No space left on disk"
date: 2023-06-20T16:01:33+10:00
type: "post"
slug: "azure-docker-no-space-left-on-disk"
tags: ["azure", "docker"]
---

This took me too long to figure out NOT to write a blog post about it 🥲

<!--more-->  

My Azure build pipeline builds several docker images as part of it's process (multiple artifacts and tests).  
For some reason, recently it started to fail with "no space left on device".  

{{< image path="img/NoSpaceLeftOnDevice" alt="No space left ☹️" >}}

I tried to slim down the images I was building, etc to no avail.  
Eventually, I came across: `docker system df` which lists what space docker "owns".  

{{< image path="img/DockerDF" alt="Such build, so yuge!" >}}

The images looks about right but wow...that build cache is yuge.  

![yuge](https://www.dictionary.com/e/wp-content/uploads/2018/05/yuge-300x200.jpg)

According to [the docs](https://learn.microsoft.com/en-us/azure/devops/pipelines/agents/hosted?view=azure-devops&tabs=yaml#hardware) Azure agents only have 10gb space for the jobs, so I'm guessing it gets a bit of wiggle room for the docker build cache.  

Regardless, my solution was to run `docker system prune -f` as a pipeline step to clear the cache half way through the process.  
That does mean the latter images needed a full rebuild, but it also means the build works ¯\\\_(ツ)\_/¯  