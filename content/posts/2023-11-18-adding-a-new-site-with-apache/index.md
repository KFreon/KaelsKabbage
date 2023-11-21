---
title: "Adding a new site with Apache"
date: 2023-11-18T19:58:29+10:00
type: "post"
slug: "adding-a-new-site-with-apache"
tags: ["apache", "selfhosted", "homelab"]
---

A while back, I started playing with [Nextcloud](https://nextcloud.com/) and wrote a bit [about it](http://localhost:1313/posts/nextcloud-journey-so-far/) noting that I wondered if I'd use it.  
Well I haven't really, but as many have mentioned to me, the whole self-hosting thing is addictive!  
So...I've been playing with [Jellyfin](https://jellyfin.org/), [Plausible](https://plausible.io/), and [VSCode/Coder](https://github.com/coder/code-server) but I keep forgetting how to set them up.  
So far, they've all been similar in their configuration and setup with my existing reverse proxy [Apache2](https://httpd.apache.org/).  
I'm going to quickly document the setup here for future reference.  

<!--more-->  

I definitely don't know much about Linux, Apache, or reverse proxies whatnot, so this approach could be full of holes. 

> Use at own risk!

{{< image path="img/I-Have-No-Idea-What-I-Am-Doing-Dog" alt="I have little idea what I'm doing" >}}

# Process
```bash
# Add A record in domain DNS

# Maybe needed if not already done
sudo a2enmod proxy proxy_http proxy_ajp remoteip headers ANY_OTHER_PLUGINS
sudo systemctl restart apache2

# create/copy site config
touch /etc/apache2/sites-available/mysite.conf
sudo cp /somepath/mysite.conf /etc/apache2/sites-available/

# Activate site
sudo a2ensite mysite.conf
sudo systemctl restart apache2

# Begin site SSL config
sudo certbot --apache

# Allow SOMEUSER user to access media math
sudo setfacl -m user:SOMEUSER:rxw /PATH
```

# mysite.conf  

```xml
<VirtualHost *:80>
    ServerName MY_DOMAIN_NAME

    # This tends to change depending on what the site needs
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/

    ErrorLog /var/log/apache2/MY_DOMAIN_NAME-error.log
    CustomLog /var/log/apache2/MY_DOMAIN_NAME-access.log combined
</VirtualHost>
```