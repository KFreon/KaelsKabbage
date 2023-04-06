---
title: "Nextcloud: LAN only"
date: 2023-04-04T13:39:36+10:00
draft: true
type: "post"
slug: "nextcloud-lan-only"
tags: ["cloud"]
---

A few times during debugging sessions, some of my colleagues have brought up this fairly cool looking notes app.  
It turns out to be far more than a notes app, and is in fact, an entire personal cloud called [Nextcloud](https://nextcloud.com/).  
While it looked and felt cool (my own cloud!), I wasn't sure I'd really make use of it, so before purchasing anything, I figured I'd set it up locally.  

<!--more-->  

# Nextcloud  
Nextcloud feels like a cool idea, host your own cloud, where you're in control, no companies selling all your data.  
It feels like a cool idea to have some central place for myself and my partner to use without having to integrate our accounts in more ways than just Photos and Calendar.  

Justification for it's existence made, I figured I'd get it up and running for us to play with.  
It wasn't as easy as it felt like it should have been, however, it IS that easy, it's just not clear how to do it.  

## AIO setup  
The options were "Enterprise", "All-in-One Docker", and "Community Projects".  
Now, maybe I'm just a Linux/FOSS noob, but I figured the easiest fastest way to get up and running was the AIO Docker image.  
I was mistaken.  

### Docker  
I hadn't setup docker on Linux before, but it was more of a pain than expected as well.  
There were a few permissions issues, but the main one was that the service wasn't started, which took me a while to figure out.  

I did eventually get docker working, and got the AIO install running, but as soon as we get to the main install screen I'm prompted to setup a domain.  
I don't want to setup a domain right now, but it won't accept `<hostname>.local`.  

## Snap install
Buried in the "Community projects" section, is the Snap install.  
This was a snap to use, and doesn't require me to setup a domain/certificate.  
There are a few things I needed to do:  
- Add my local ip range to Nextcloud as trusted domains
  - `/var/snap/nextcloud/current/config/config.php` --> trusted_domains --> '10.0.0.*'  
- Expose port 80 in firewall  
  - `dnf install firewall-config`  
  - Add 80/TCP  
  - This is required when trying to access from another device on LAN  

## After installation  
I don't want to setup an email server for user management either, so in Users --> Settings --> uncheck "send emails to new users".  
Otherwise the new user is created but not activated, and can't be logged into yet.   

## Adding external files  
To get external hdd support, you'll need `sudo snap connect nextcloud:removable-media`.  
Once I moved things, I also needed to go `sudo nextcloud.occ files:scan --all` to get them picked up.  

I use [Resilio Sync](https://www.resilio.com/individuals/) to sync some files around my local network, so I setup a sync of my photos to the server.  
> The main sync setup is on my Pi with big HDD's plugged in, but currently this Nextcloud setup is on an old laptop, hence requiring the sync.  

The sync goes to it's own folder, in my case `/home/rslsync/Resilio\ Sync/Camera`.  
> I tried to put that somewhere more sensible, but `Kael + Linux != success`.  
I tried to use Nextcloud's [External storage support](https://docs.nextcloud.com/server/latest/admin_manual/configuration_files/external_storage_configuration_gui.html) "app" (which I needed to enable) to create a link to this folder, but it didn't like the `Resilio\ Sync` part, so I made a symlink: `ln -s /home/rslsync/Resilio\ Sync/Camera /home/rslsync/my-camera`.

Once that was done, it all worked!  
> Not sure why it was picked up immediately when the other files weren't. Perhaps enabling that app added another cron job to check files?  