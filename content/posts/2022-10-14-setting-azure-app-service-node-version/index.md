---
title: "Setting Azure App Service node version"
date: 2022-10-14T19:34:43+10:00
type: "post"
slug: "setting-azure-app-service-node-version"
tags: ["node", "Azure"]
---

I wanted to use the Azure App Service console to install some software to test which needed NodeJS.  
The default Node version is `0.10.1`...  

<!--more-->  

This post won't go into how to install a particular node version (I didn't need specifics), just how to set one of the default installations.  
Microsoft [recommends](https://techcommunity.microsoft.com/t5/apps-on-azure-blog/windows-azure-app-service-nodejs-version/ba-p/3102319) NOT setting minor versions, so I'll just set 16.  

# Find supported NodeJS versions  
`htts://YOURSITE.scm.azurewebsites.net/api/diagnostics/runtimes`
> In the Azure Portal: Advanced Tools --> Runtimes  

# Set the version  
- In Azure Portal: Configuration --> Application Settings.  
- Create new setting `WEBSITE_NODE_DEFAULT_VERSION` and set to `~16` to use the latest minor version of `16.x`  

Your app will restart with a REAL version of NodeJS :D  