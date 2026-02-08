---
title: "Selfhosting: What I'm using, not using, and why"
date: 2026-01-29T18:32:03+10:00
type: "post"
slug: "selfhosting-what-im-using-not-using-and-why"
tags: ["selfhosted", "linux"]
---

While I've [had time to play with my homelab](/content/posts/2026-01-23-external-speedtest-for-home-assistant/index.md), I figured I'd do a basic list of what I have and why.  
I'm not going ham on this yet. I've seen people with server racks, PC's for days, and hard drives forever and I'm just trying to use whatever I have.  
If it becomes something we use all the time, I'll get more elaborate, but for now, I'm the only one using it...  

<!--more-->  

{{% toc levels="three" %}}

# Infrastructure and general setup  

- Old work laptop under my desk running as primary homelab (Fedora 43) 
  - Almost everything runs in Docker  
  - It was before I learned about Proxmox :(  
- [BinaryLane](https://www.binarylane.com.au/) VPS running Nextcloud and a few other things  
- Raspberry Pi 3 running Pihole, and another running Home Assistant OS  

Broadly, public facing stuff is on the VPS, and all my data is synced to the old laptop, then backed up to Azure blob storage.  
Then anything else is accessed via Tailscale.    

# Primary Homelab  

## [Waypipe](https://github.com/deepin-community/waypipe)  
Not really selfhosting, more Linux, but a successor to `ssh -x` where you can execute GUI apps and forward it to the host.  
e.g. with waypipe installed on both host and client, I can go `waypipe ssh me@host nautilus` and I'll get the standard Fedora Files app open, but it's actually the one on the host.  
Pretty cool stuff.  
The most fun use case I had was that I needed to configure something on HomeAssistant, but I couldn't get to it via Tailscale (it is the exit node, and the addon wasn't running) so I waypiped Firefox on another machine at home and just went to the HomeAssistant LAN address as usual.  
I did need `--no-gpu` for Firefox though.  
`waypipe --no-gpu ssh me@host firefox`  

## [Homepage](gethomepage.dev)  
It's fairly simple and has nice widgets.  
Since I'm not doing reverse proxy on the homelab (yet) I always forget which port is which.  
This way, I don't need to remember, and I can have a nice landing page that I can style. 
 

## [Glances](https://github.com/nicolargo/glances) (not [Glance](https://github.com/glanceapp/glance) but it is on my list)  
System monitor, and plugs into Homepage to give me nice "at a glance" charts to see how it's going.  
This was particularly handy when I was ambitious with Frigate and way overloaded it.  
Also when I ssh'd to `~` via VSCode and it tried to index most of the drive...  

> `rg` = ripgrep apparently and it's part of search and indexing, and even though I've excluded all files from search and whatnot, I still can't get it to stop doing it on connect.  

## [Frigate](https://frigate.video/)  
Network recording and alert/detection for cameras.  
I wanted to play with basic security cameras (got some cheap Tapo ones to play with) and be able to detect when people were in my driveway.  
Works fairly well, but getting GPU passthrough for Docker working was a pain.  

## [Jellyfin](https://jellyfin.org/)
Movies and TV show server.  
This was the impetus for doing any of this.  
It's so good and is mostly fine (GPU passthrough is also a pain) but have had issues with some weird browsers failing (Xbox Edge for example)  

## [Kopia](https://kopia.io/)  
Backup software.  
Fast, incremental, and encrypted.  
Generally simple and has support for lots of external services, etc, but its concept of a repository is a bit weird to get my head around.  
It feels especially odd that you can't easily switch repositories in the UI (yet)  
I've got this backing up to Azure Blob Storage every 15 mins.  
Costs like $10AUD/month for 150gb, so it's a fairly cheap and safe backup mechanism (I hope!).  

## [Olive Tin](https://www.olivetin.app/)  
Runs commands on host.  
This is the only one that runs on the metal because I couldn't get it working in Docker.  
I have it setup to run some scripts on the host like `mount -a` cos sometimes my drives disconnect for some reason.  

## [Komodo](https://komo.do/)  
System/Docker manager.  
This is some nice magic to help administor containers and the rest of the system via web.  
I didn't have anything initially, and I had to ssh to get docker logs and restart containers.  
Now it's just a few clicks. Very nice. Do recommend.  
I believe it's a competitor to Portainer.  


# Raspberry Pis  

## [Home Assistant](https://www.home-assistant.io/)  
Home automation and such running on a Pi3.  
It's mostly used to have a single place to manage all my smart devices of different brands.  
Main usages:  
- smart plugs to monitor power usage  
  - [Tapo P110](https://www.tp-link.com/au/home-networking/smart-plug/tapo-p110/) NOT P100, which are only smart, not power monitoring  
- smart plugs to add smarts to dumb lights (like christmas tree lights)  
- weather station info  
  - [Ecowitt WS3900C and co](https://www.amazon.com.au/ECOWITT-WH2910C-Weather-Wireless-Forecast/dp/B07FD3DSXJ?crid=Z55FOPJZKIKH&dib=eyJ2IjoiMSJ9.nO-Huv_hvxP4AwG3qhBSBL4T_aAaaGmZxbxTCOntJQkHEjgXyl_9hS_LhKokK0Leej4Tnt6vkL4SNPGTD7LE5VgaoIpZHBLPqVwnJzIGWP8.LYus4dQ21tT1eAdrxjFKEEQQ6FNeN67ElryWlHGRTDU&dib_tag=se&keywords=WS3900C&qid=1770521775&s=home-garden&sprefix=ws3900c%2Cgarden%2C276&sr=1-1&th=1)
- contact, presence, motion, etc sensors to control lights based on events  
  - Tapo T110, T100, etc
- temperature tracking of nursary, etc  
  - [Switchbot Temperature meter](https://www.switch-bot.com/products/switchbot-meter)

I really want to control the aircon, but it's too old 🙁  

## [Pihole](https://pi-hole.net/)  
Network-wide ad blocking.  
I had trouble with this where my router doesn't allow changing DNS, so I had to make the pihole my DHCP server too.  
Seems to work pretty well.  

### Nits  
A fun aside, with Pihole DNS I seem to have to add `.local` to the hostname.  I'm not sure why really, I don't know much about it.  
But what that means is that I needed to add Javascript to Homepage to adjust it's links based on whether I'm coming from LAN or Tailscale.  (Lan was HOST.local, Tailscale was just HOST like normal)  
From what I understood, it should have been `.lan` from what was configured in Pihole, but whatever.  

Another aside is that because LAN and Tailscale have different IP's and hostnames, SSH would fail as it couldn't find HOST.local on Tailscale.  
I tweaked ssh config to alias the names.  

``` bash {title="~/.ssh/config"}

# TAILSCALE
Host REALHOSTNAMEtail
  HostName REALHOSTNAME
  User ME

# LAN
Host REALHOSTNAME
  HostName REALHOSTNAME.local
  User kael
```

# VPS  

## [Plausible](https://plausible.io/)  
Selfhosted analytics for this blog.  
Basic traffic.  

## [Nextcloud](https://nextcloud.com/)  
Whole cloud replacement.  
It's a huge pain though, so I'm not sure about recommending it...  
It took me days to upgrade it from version 30-31 which required database upgrades that took ages to figure out.  
Mabybe the [AIO docker](https://apps.nextcloud.com/apps/nextcloud_all_in_one) is better but I [had issues](/content/posts/2023-04-04-nextcloud-lan-only/index.md) getting it working.  

# All of them  

## [Tailscale](https://nextcloud.com/)  
VPN to connect all my services and allow external access when I want it.  
Super simple to setup, and free!  


# Not using  

## [Arr stack](https://wiki.servarr.com/)  
Just don't need it. I read through a few setups, but it looks a bit fiddly.  

> I was looking at using [MediaStack](https://github.com/geekau/mediastack) as it was a fully fledged docker compose.  


## [Paperless-ngx](https://docs.paperless-ngx.com/)  
I used this a few times, but I couldn't get email integration working, and I couldn't find any simple "Send to paperless" integrations that worked for me.  
I also just wasn't using it...

## [Resilio Sync](https://www.resilio.com/sync/)  
Before I setup Nextcloud, I had this to do my syncing and it was fine but flakey.  
When I added Nextcloud, it had syncing built in that seems to work.  (Except sometimes on Android due to Google being trash)  

## [VSCoder](https://github.com/coder/code-server)  
VSCode that can run in the browser.  
I thought this'd be cool to have it running on my server and then I can access it from anywhere!  
It was [fiddly](/content/posts/2022-10-22-vscode-in-browser-unexpectedly-difficult/) and now there's the remote access stuff built in.  
I also never ever used it...

## [Heimdall](https://github.com/linuxserver/Heimdall)  
Decent homepage, but just couldn't "get" it so moved to Homepage.  

## [Kavita](https://www.kavitareader.com/) / [Booklore](https://booklore.org/)  
Book hosting, but most of my books are in Kindle so I don't really use them :(  

## [Donetick](https://donetick.com/)  
Selfhosted todo and such, but I already use Microsoft Todo and I feel no need to move, but I did want to play with it.  
Seems good.  