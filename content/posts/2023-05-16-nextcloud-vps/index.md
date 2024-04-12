---
title: "Nextcloud: My Journey so far"
date: 2023-05-16T20:44:35+10:00
type: "post"
slug: "nextcloud-journey-so-far"
tags: ["nextcloud"]
---

After admitting defeat on my [previous post](/content/posts/2023-04-04-nextcloud-lan-only/index.md) attempting to get Nextcloud running locally (and remembering that I wanted an offsite backup anyway), I've bought a VPS and installed Nextcloud on it.  

<!--more-->  

# Previously, on Nextcloud...  
I was adamant on just testing out Nextcloud and tried to install it locally in my network.  
This was [not easy](/content/posts/2023-04-04-nextcloud-lan-only/index.md), and ultimately I gave up.  

In short, the Snap worked fine, but I wasn't able to add HEIC support among other things, so I started doing crazy things like building the docker images from scratch with required edits, and I realised I was going too far...  
I eventually remembered I wanted an offsite backup anyway, so I gave up and bought a Virtual Private Server.

# Virtual Private Servers
A VPS seems to be a VM that you're provisioned by the provider that you're in control of.  
I hadn't heard of such a thing before, but I suppose it's no different to an Azure VM (except no graphics card, and Linux), but the other positive here is that it's not owned by Microsoft.  

## BinaryLane  
My VPS of choice is BinaryLane based on recommendations, and it does what it says on the tin.  
It was easy to purchase and easy to setup.   

## Specs
- 1 cpu
- 1gb RAM
- 500gb HDD
- ~$16/month
- Brisbane
- Ubuntu

> I eventually had to change this to 2gb RAM and 100gb HDD due to constant out of memory exceptions when I added all my Photos... ☹️

# Nextcloud Installation  
I started with the recommended install method, a `setup_nextcloud.php` page, but I didn't have a webserver to serve it...which took me too long to realise.  

The next recommended method is the Nextcloud VM install script, and despite this not being a VM, worked fairly well.  
> I had to remove the 2gb RAM check, but it seems to work ok, except that there are Nextcloud plugins which won't install or function due to the low RAM available, notably any AI tools like the image recogniser.  
> This turned out to be insufficient for many operations, like photos and I had to up the RAM to 2gb.

The rest of the setup was simple enough, just following the wizard including allowing the build-in SSL certificate setup via LetsEncrypt.  

At this point, Nextcloud is working!  
However, I have my backups and data on my HDD's on my local network, and that's the data I wanted in Nextcloud, so what's a good way of getting it up there?  

# Resilio Sync Setup  
I like [Resilio Sync](https://www.resilio.com/individuals/), a peer to peer sync program based on Bittorrent, with clients on many platforms.  
It's setup is generally pretty simple on each platform as well.  

## WebUI Access via IP
By default, it's webUI isn't exposed outside of `localhost`, so in `/etc/resilio-sync/config.json`, I set the `webui` section to:

```json
"webui": {
  "force_https": true,
  "listen": "<MY VPS IP>:8888"
}
```

Now I can navigate there via that IP (or hostname relating to that IP), but I get an SSL warning about untrusted certificate.  
It turns out [this is expected](https://help.resilio.com/hc/en-us/articles/4404757430291-Browser-warning-Your-connection-is-not-private-) as it's using a self-signed certificate.  
Normally, I'd be ok with just ignoring the warning, but if I navigate to `https://<MY NEXTCLOUD DOMAIN>.com:8888`, I get a HSTS error and am unable to proceed.  
I'd also prefer to have it "just work".  

<div class="tenor-gif-embed" data-postid="20598651" data-share-method="host" data-aspect-ratio="1.78771" data-width="50%"><a href="https://tenor.com/view/todd-howard-it-just-works-bethesda-this-all-just-works-gif-20598651">Todd Howard It Just Works GIF</a>from <a href="https://tenor.com/search/todd+howard-gifs">Todd Howard GIFs</a></div> <script type="text/javascript" async src="https://tenor.com/embed.js"></script>

## WebUI Access via new desired hostname  
The [Resilio docs](https://help.resilio.com/hc/en-us/articles/4404757430291-Browser-warning-Your-connection-is-not-private-) indicate that I can override the default self-signed certificate in `config.json` as below:  

```json
"webui": {
  "force_https": true,
  "listen": "<MY VPS IP>:8888",
  "ssl_certificate": "/etc/letsencrypt/live/MY NEXTCLOUD DOMAIN/fullchain.pem",  // Note that the live certs don't work for me here.
  "ssl_private_key": "/etc/letsencrypt/live/MY NEXTCLOUD DOMAIN/privkey.pem"     // I suspect it's because they're symlinks and it can't follow them?
}
```  
> This certificate and key are the ones that Letsencrypt created during the Nextcloud setup.  

Those certificate files are owned by `root` (by default anyway) and the Resilio service isn't allowed to access them (user: `rslsync`).  
This took some time to figure out, but the following worked:

```bash
# Add default user to rslsync group and vice versa
# This is the recommended and documented approach: https://help.resilio.com/hc/en-us/articles/206178924-Installing-Sync-package-on-Linux
# However, I note here that the user I was using is the root user...and this is probably a really bad idea...
sudo usermod -aG <myuser> rslsync
sudo usermod -aG rslsync <myuser>

# Here lives all the Letsencrypt certificates created by certbot during Nextcloud install
cd /etc/letsencrypt

# Add read AND execute permissions to all files and folders in the certificates folder
# If you have multiple letsencrypt certificates, you can and should apply these permissions more granually.
sudo chmod g+rx -R archive
sudo chmod g+rx -R live   # Live points to the current certificate in archive. Auto-adjusted when renewed.

# Required to start resilio-sync service. You may have already done this.
sudo systemctl enable resilio-sync

# Required after changes to config.json
sudo systemctl restart resilio-sync
```  
> I'm not sure why `x` is required, but it is.  
> Likely server restart is required, but apparently there is a way to refresh the shell permissions as well.  

Now I can go to `https://MY NEXTCLOUD DOMAIN.com:8888` without any issues, and receive the Nextcloud certificate.  

<a href="https://imgflip.com/i/7m7vec"><img src="https://i.imgflip.com/7m7vec.jpg" title="made at imgflip.com"/></a><div><a href="https://imgflip.com/memegenerator"></a></div>

# But how to get the synced files?  
In the end, I opted to do `sudo usermod -aG rslsync www-data` and vice versa thus allowing Nextcloud to access synced files.  
However, the default sync location is `/home/rslsync/SOMEWHERE` which isn't really accessible to the Nextcloud UI. It tends to be from it's data root only.  
So we can enable a feature called [External Storage](https://docs.nextcloud.com/server/latest/admin_manual/configuration_files/external_storage_configuration_gui.html) in Nextcloud --> Apps --> Your Apps.  
> It's installed by default and doesn't show in any of the categories except "Your Apps"  

After it's enabled, you can add a directory anywhere and we now have access to the synced directory.

{{< image path="img/ExternalSetup" alt="Nextcloud External Setup" >}}

I ended up having to run `sudo -u www-data occ files:scan --all` to get it to be detected though.  

# Am I happy now?    
We'll see, this whole thing was just a trial to figure out if I even wanted to sink more time into setting up something cool...  
I've learned more about Linux, infrastructure, and various other little things along the way, so I'd say it's a net positive!  