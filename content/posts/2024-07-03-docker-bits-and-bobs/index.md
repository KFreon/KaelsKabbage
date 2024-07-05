---
title: "Docker bits and bobs"
date: 2024-07-05T12:16:38+10:00
type: "post"
slug: "docker-bits-and-bobs"
tags: ["docker"]
---

Docker has become a widely used tool in the industry, but I keep forgetting how to do even it's most simple commands.  
Google is sufficient for those, however there's a few things I want to write down for my future self.  

{{% toc %}}

<!--more-->  

# Override the entrypoint  
I do this all the time so I can see what's inside an image, usually for debugging.  
`docker run -it --entrypoint /bin/bash IMAGE`  
`-it` is "interactive terminal".  

# Proxy  
`HTTP_PROXY`, `HTTPS_PROXY` and `NO_PROXY` don't generally have to be explicitly set in your docker images.  
That is, you don't need to declare `ARG HTTP_PROXY` or `ENV HTTPS_PROXY`. They should already be there.  

`docker build --build-arg HTTP_PROXY=http://102.35.11.25:1145`  

# How much space does docker take up?  
`docker system df` shows the disk consumption of docker.  
`docker system prune -f` deletes unused images.  
[I had an issue](/content/posts/2023-06-20-azure-docker-no-space-left-on-disk/index.md) on a build agent where I ran out of disk space, and I used this to clean it up.  
The build cache on my local machine is 18gb+ right now!  

# Install node on a non-node image  
I had the dotnet SDK image and wanted to install node on it.  
Should I have just done it differently? Maybe, but here's how to install it anyway.  

```javascript {title="Installing node 18 on a non-node image"}
RUN set -uex; \
    apt-get update; \
    apt-get install -y ca-certificates curl gnupg; \
    mkdir -p /etc/apt/keyrings; \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key \
     | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg; \
    NODE_MAJOR=18; \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" \
     > /etc/apt/sources.list.d/nodesource.list; \
    apt-get update; \
    apt-get install nodejs -y;
```  

# PUID and PGID  
These are Linux user and group mappings that allow smooth security mappings with your docker images.  

[Linuxserver.io](https://docs.linuxserver.io/general/understanding-puid-and-pgid/) have a writeup that explains it more completely, however my basic understanding is that these control how the container interacts with the host.  
I assumed that the process inside the container had this user/group value, but as I understand it, they're separate.  
The user things run under in the container is defined by the `USER` directive in the image.  
Without `USER`, [the default](https://www.docker.com/blog/understanding-the-docker-user-instruction/) is `root`, which can be insecure because then the process can do whatever it wants inside the container, including accessing any resources the container has access to (like volumes and such)  

If you set the PUID to your host user (on linux at least) e.g. `docker run -e PUID=1000 myimage`, the process in the container will be running under the container user `root` but that user will have the permissions of user `1000` for resources mapped in from the host.  
e.g. if you have a volume mapped in, anything created by the container `root` user will be done under the host user `1000`.  

This is good for making sure that your user on the host actually has access to the things being written to volumes.  

> I feel like I don't have a good grasp of this, or even Linux users and permissions in general, so take with several grains of salt.  

# Secrets (in docker compose)  
Many containers have username and password or api key requirements, and in Docker compose files, these are usually environment variables.  
We don't want these as part of the repository, so we want to be able to point to it, but have it outside the repo.  

Some of these are setup as the path to a file on the host such that you can copy in a secret file (like the mssql image) and it will read it in, however this isn't standard (yet)  
Docker compose can do this secret copying in a standard way:  

```yaml
services:
  das: 
    build:
      context: .
      dockerfile: secrets_dockerfile
    secrets:
      - mysecret

secrets:
  mysecret:
    file: ./mysecret
```

`/mysecret` is copied into `/run/secrets/mysecret` inside the container and it's contents should just be the secret and nothing else.  
However, how does the application get that secret if it's looking in an environment variables.  

I've been creating a custom dockerfile for the applications and changing it's entrypoint to a script that writes the contents of the secret into the environment variable.  

```docker {title="Custom dockerfile for NET 8 SDK"}
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN mkdir /app
COPY ./set_env_var.sh /app
RUN ["chmod", "+x", "/app/set_env_var.sh"]
ENTRYPOINT ["bash", "/app/set_env_var.sh"]
```

```sh {title="set_env_var.sh"}
export MAHSEKRET="$(cat /run/secrets/mysecret)"
# This would be the actual command you want to run as the entrypoint.
# Inspect the target image to see what it's entrypoint is so you can use it here.
"/bin/bash" "-c" "echo $MAHSEKRET" 
```
