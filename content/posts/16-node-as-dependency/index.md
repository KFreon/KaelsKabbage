---
title: "Node as a Dependency"
date: 2019-06-30T09:50:37+10:00
draft: false
type: "post"
slug: "node-as-a-dependency"
tags: ["node"]
---

Managing node versions can become painful when switching between projects.  
There's a mechanism to check node versions during an {{< inline "npm install" >}} called [Engines](https://docs.npmjs.com/files/package.json#engines), but that doesn't help if the projects are installed already.  
The fix is to install node as a dependency. Who would have thought?  

<!--more-->  

## The Problem  
Let's say you have several projects you're working on for clients and each one has different versions of node.  
The current way of working (for me anyway) is to use [NVM](https://github.com/coreybutler/nvm-windows) to switch versions of node.  
It works, but is tedious and can become confusing.  

## The Solution  
{{< inline "npm install node@<version> --save-dev" >}}  
That's it. Any npm commands are then run using the locally installed node version.  

My currently installed version of node is 10.15.3, so running {{< inline "node -v" >}} as part of an npm script gives me the following:  
{{< image path="img/Node_Before" alt="Node version is the globally installed node version" >}}  

After installing node as a dependency, the build uses the local version instead!  
{{< image path="img/Node_After" alt="Now it's using the locally installed version" >}}

Enjoy!