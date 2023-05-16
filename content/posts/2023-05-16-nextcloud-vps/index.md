Add more info about what I need to do to get synced files to be visible in Nextcloud.

Add screenshots about sync certificate errors?

reword/rewrite?





---
title: "2023-05-16-nextcloud-vps"
date: 2023-05-16T20:44:35+10:00
draft: true
type: "post"
slug: "2023-05-16-nextcloud-vps"
tags: ["nextcloud"]
---

After admitting defeat on my [previous post]({{< ref "/posts/2023-04-04-nextcloud-lan-only/index.md" >}}) attempting to get Nextcloud running locally (and remembering that I wanted an offsite backup anyway), I've bought a VPS and installed Nextcloud on it.  

<!--more-->  

# Intro  
I was adamant on just testing out Nextcloud, and decided to install it locally in my network.  
This was not easy, and ultimately I gave up.  
The Snap worked fine, but I wasn't able to add HEIC support among other things, so I started doing crazy things like building the docker images from scratch with required edits, and I realised I was going crazy...  
I eventually remembered I wanted an offsite backup anyway, so I gave up and bought a Virtual Private Server.


# BinaryLane  
My VPS of choice is BinaryLane based on recommendations, and it does what it says on the tin.  

## Specs

- 1 cpu
- 1gb RAM
- 500gb HDD
- ~$16/month
- Brisbane

# Nextcloud Installation  
I started with the recommended install method, a `setup_nextcloud.php` page, but I didn't have a webserver to serve it...which took me too long to realise.  

The next recommended method is the Nextcloud VM install script, and despite this not being a VM, worked fairly well.  
> I had to remove the 2gb RAM check, seems to work ok, but there are Nextcloud plugins which won't install or function due to the low RAM available, notably any AI tools like the image recogniser.  

The rest of the setup was simple enough, just following the wizard including allowing the build-in SSL certificate setup via LetsEncrypt.  

At this point, Nextcloud is working!  
However, I have my backups and data on my HDD's on my local network, and that's the data I wanted in Nextcloud, so what's a good way of getting it up there?  

# Resilio Sync Setup  
I like [Resilio Sync], a peer to peer sync program based on Bittorrent, and I set it up as described in it's setup instructions.  
Nothing crazy yet...  

## WebUI Access via IP
It's webUI isn't exposed outside of `localhost` by default, so in `/etc/resilio-sync/config.json`, I set the `webui` section to:

```json
"webui" : {
  "force_https": true,
  "listen": "xxx.xxx.xxx.xxx:8888"
}
```

Now I can navigate there via that IP (or hostname relating to that IP), but I get an SSL warning about untrusted certificate.  
It turns out [this is expected](https://help.resilio.com/hc/en-us/articles/4404757430291-Browser-warning-Your-connection-is-not-private-).  
However, if I try to go there based on the hostname in the certificate I got from the Nextcloud setup, I get a HSTS error and I can't even ignore and proceed.   

## WebUI Access via new desired hostname  
At this point, I'm feeling like a video game character on a fetch quest...  
The docs above indicate that I can override the default self-signed certificate in `config.json` as below:  

```json
"webui" : {
  "force_https": true,
  "listen": "xxx.xxx.xxx.xxx:8888",
  "ssl_certificate": "/etc/letsencrypt/live/MYNEXTCLOUDDOMAIN/fullchain.pem",
  "ssl_private_key": "/etc/letsencrypt/live/MYNEXTCLOUDDOMAIN/privkey.pem"
}
```  
> Certificate and key are the ones that Letsencrypt created during the Nextcloud setup.  

I still need to give permissions for the Resilio Sync user `rslsync` to these certificate files.  
This took some time to figure out, but the following worked:

```bash
# Add default user to rslsync group and vice versa
# This is the recommended and documented approach: https://help.resilio.com/hc/en-us/articles/206178924-Installing-Sync-package-on-Linux
# However, I note here that the user I was using is the root user...and this is probably a really bad idea...
sudo usermod -aG <myuser> rslsync
sudo usermod -aG rslsync <myuser>

cd /etc/letsencrypt

# Add read AND execute permissions to all files and folders in the certificates folder
# If you have multiple letsencrypt certificates, you can and should apply these permissions more granually.
sudo chmod g+rx -R archive
sudo chmod g+rx -R live   # Live points to the current certificate in archive. Auto-adjusted when renewed.

# Required at some point to start resilio-sync service
sudo systemctl enable resilio-sync

# Required after changes to config.json
sudo systemctl restart resilio-sync
```  
> I'm not sure why `x` is required, but it is.  
> Likely server restart is required, but apparently there is a way to refresh the shell permissions as well.  

Now I can go to `https://MYDOMAIN.com:8888` without any issues, and receive the Nextcloud certificate.  

# Conclusion  
I feel like something about the setup of the Nextcloud certificate was specific or unusual as I couldn't find much else about this kind of setup.  

Regardless, now I can add my files and folders to sync up from my local network to my VPS and have Nextcloud see them!