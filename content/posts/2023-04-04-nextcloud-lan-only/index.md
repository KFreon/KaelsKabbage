---
title: "Nextcloud: LAN only"
date: 2023-04-04T13:39:36+10:00
type: "post"
slug: "nextcloud-lan-only"
tags: ["cloud"]
---

A few times during debugging sessions, some of my colleagues have brought up this fairly cool looking notes app.  
It turns out to be far more than a notes app, and is in fact, an entire personal cloud called [Nextcloud](https://nextcloud.com/).  
While it looked and felt cool (my own cloud!!), I wasn't sure I'd really make use of it, so before purchasing anything, I figured I'd set it up locally.  
This took longer than I expected.   

<!--more-->  

# Nextcloud  
Nextcloud feels like a cool idea, host your own cloud, where you're in control, no companies selling all your data.  
It feels like a cool idea to have some central place for myself and my partner to use without having to integrate our accounts in more ways than just Photos and Calendar.  

Justification for it's existence made, I figured I'd get it up and running for us to play with.  
It wasn't as easy as it felt like it should have been, but it IS that easy, it's just not clear how to do it.  

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
> All the above Docker setup is not required for this part.  

Buried in the "Community projects" section, is the Snap install.  
This was a 'snap' to use, and doesn't require a domain/certificate to run locally.  
There are a few things I needed to do:  
- Add my local ip range to Nextcloud as trusted domains
  - `/var/snap/nextcloud/current/config/config.php` --> trusted_domains --> '10.0.0.*'  
- Expose port 80 in firewall  
  - `dnf install firewall-config`  
  - Add 80/TCP  
  - This is required when trying to access from another device on LAN  
> The firewall thing took the most time to realise, and required asking colleagues. I can't believe I didn't think of it.  

## After installation tips and tricks  
  - Users --> Settings --> uncheck "send emails to new users"
    - Allows new users to log in without activation
  - Add external drives
    - `sudo snap connect nextcloud:removable-media`  
  - Files not showing up when added to server directly
    - I moved files to the Nextcloud data directory using [Resilio Sync](https://www.resilio.com/individuals/) but they weren't detected.
    - `sudo nextcloud.occ files:scan --all` picked them up
  - Some issues with spaces in the path
    - The sync goes to it's own folder, in my case `/home/rslsync/Resilio\ Sync/Camera`
    - I tried to put that somewhere more sensible, but `Kael + Linux != success`.  
    - I tried to use Nextcloud's [External storage support](https://docs.nextcloud.com/server/latest/admin_manual/configuration_files/external_storage_configuration_gui.html) "app" (which I needed to enable) to create a link to this folder, but it didn't like the `Resilio\ Sync` part, so I made a symlink: `ln -s /home/rslsync/Resilio\ Sync/Camera /home/rslsync/my-camera`.
    - > These were picked up immediately, but others weren't, not sure why.  



# Worth it?  
Was all this worth it? Do we use it?  
Well...not yet, but surely soon right?  
Ah well, if not, it was fun 😀