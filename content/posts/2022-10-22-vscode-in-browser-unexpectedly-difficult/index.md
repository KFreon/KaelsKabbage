---
title: "VSCode in Browser: Unexpectedly difficult"
date: 2022-10-24T13:52:39+10:00
type: "post"
slug: "vscode-in-browser-unexpectedly-difficult"
tags: ["vscode", "azure"]
---

Can I provide a fully powered IDE experience to users without external requirements?  
I figured there must be something like the Github `.` experience that I could use.
Apparently I was wrong.  

<!--more-->  

# TLDR  
- Coder [Code-Server](https://github.com/coder/code-server) and it's [docker image](https://github.com/bpmct/code-server-azure) in an Azure App Service running Linux Containers  
- Azure File Mount for code share  
- It's a decent time

# What I want
- Rich C# dev experience
- Ability to have a link that navigates to the precise file/folder I want to show
- All of this to be automatic for the user, no additional fiddling

# Why did I want this?
For better or worse, I decided I wanted to play with Blazor by automating code puzzle submissions.  
After writing a Blazor Server app to do all that testing, I realised the next obvious step was to actually show the code.
This post will cover the basic thought/attempts to get from here, to the solution I ended up with.  

# Ideas and Attempts
## Monaco  
I knew I didn't just want a rich text field, I wanted syntax highlighting, code folding, but most importantly: **Go to definition**, and similar levels of code navigation.  
Monaco doesn't have such navigations, it's *just* the editor.  
I briefly tried it anyway, and it was easy to get running in Blazor.  
It wouldn't be too hard to implement the tree view, etc, but I kept coming back to the lack of navigations.
> Viewing the code isn't enough. I wanted those proper IDE features.  

## vscode.dev?  
Remembering that Github has VSCode integration now, I figured I'd be able to use [vscode.dev](https://vscode.dev) somehow.  
After googling and fiddling of my own, I realised that the browser wouldn't be able to access code on some other service, particularly not automatically.  
I also couldn't find any way of embedding it myself in a way that let me show arbitrary code.  

> Sidenote here, I COULD have had a git repo in which are all the submissions that we then can go to and hit `.` to get this experience.
> While this is still an option, this was a PD project ,and I wanted the best!

## Microsoft Code-Server
[Not released yet](https://code.visualstudio.com/blogs/2022/07/07/vscode-server)  
Also looks like it needs github account.


## SSH/Remote container tools for local VSCode  
This was my least-favourite option off the bat because I assumed it would need manual steps.  
As far as I can tell, I wasn't wrong :(

**To elucidate:**  
- Requires VSCode on the users machine (not really an issue)
- Requires custom (in Preview) setup on the app service
  - Also seems to be per-session manual setup
- Doesn't seem to be any url access/triggering capabilities, making it less useful for my purposes
  - Disregarding the manual setup, I could have displayed a session key which the user could then have setup access with their VSCode, but that was already more friction than I wanted, even if I could figure out how to do it

# The Eventual End Result: **Coder Code Server**
I found a docker image which has been built from the Microsoft base image which can be run using Azure Webapp For Linux Containers.  

[Coder Code-Server](https://github.com/coder/code-server) isn't quite VSCode, but it's close enough for my purposes.  
It provides `settings.json` access to configure settings when building the image as well which is nice for setting themes, etc.  
While it supports a github link where it gets a proper url on another domain and uses github access control, I didn't want to make all users login with github, grant access, etc.
Fortunately, it supports password authentication, which is secure enough for this project.

There was some work to do though.  

## Getting the code on the machine
After some fiddling and googling, Azure File Mounts turned out to be a suitable method to get the code on the required app service by mounting one to both App services.
It's in Preview for Windows App Services, but it seems to work fine. 

> Why didn't I run it on the same app service?  
> - First, you can't. Only one set of ports exposed for Linux WebApps.  
> - Second, the docker image docs cover Linux only (unsure if it would run on Windows) but my other projects' app service needs to be Windows for NET Framework support. 

## Opening to file/folder I want
Code-Server provides functionality to open a folder via query string parameter out of the box, so that's a simple one ticked off!  

## Rich C# code support
Since the image isn't a Microsoft VSCode implementation, we don't have C# language support built in :(

### Getting the dotnet SDK
The Coder image doesn't actually HAVE dotnet installed in it, so I need to install that as part of the build as well.  
I use an [install script](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script) to install it as part of the build, where it's installed to `/root/.dotnet`

### C# Extension
There IS an [Omnisharp extension](https://open-vsx.org/extension/muhammad-sammy/csharp) in the [open-vsx](https://open-vsx.org/) ecosystem but it does work well enough, although NET 6 only without additional configuration.  
So for now, only NET 6 C# is properly navigatable, but I consider that ok for now.  

I don't want users to have to install and configure the C# extension when they visit the site so:
- Copy the installed extension out, and as part of the Docker build, copy it to the extensions directory  
- Update the included `settings.json` in the build to set the Omnisharp C# root path to the above install directory:
```json
{
  //...
  "omnisharp.dotnetPath": "/root/.dotnet",
  //...
}
```  

It works pretty well, if a bit slow on my one-step-up-from-free tier, but it does work!  

### Issue with Globalisation support  
There was an issue with globalisation support which needed an environment variable `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` set to `1` to bypass.  

# Some notes
- I had issues with existing bin/obj folders breaking Omnisharp. Deleting them solved the issue.
- Omnisharp is slow on my tier, meaning sometimes it takes a minute or two for the intellisense/code navigation to work.
- The resulting image is quite large > 1.5gb


# Resulting DockerFile 
```docker
FROM codercom/code-server:latest as final

USER coder

# Apply VS Code settings
COPY settings.json /root/.local/share/code-server/User/settings.json

# Use bash shell
ENV SHELL=/bin/bash

# Required to fix globalisation issues
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Ensure it runs on port 80
ENV PORT=80

# Use our custom entrypoint script and our python server
COPY azure-entrypoint.sh /usr/bin/azure-entrypoint.sh
COPY miniRedirectServer.py /home/coder/miniRedirectServer.py

COPY dotnet-install.sh /usr/bin/dotnet-install.sh
COPY extensions /root/.local/share/code-server/extensions

USER root
EXPOSE 80 2222

# Fix permissions
RUN chown -R coder:coder /home/coder

# Fix SSH bug
RUN mkdir -p /var/run/sshd
RUN mkdir /home/coder/project

# Install dotnet SDK
RUN chmod 777 /usr/bin/dotnet-install.sh
RUN /usr/bin/dotnet-install.sh -c Current

# This is what's run in cloud
ENTRYPOINT ["/usr/bin/azure-entrypoint.sh"]

# This is required locally for some reason
# ENTRYPOINT ["bash", "/usr/bin/azure-entrypoint.sh"]

# Debugging, but container must be made from Docker...probably needs -it or something
# ENTRYPOINT ["bash"]
```