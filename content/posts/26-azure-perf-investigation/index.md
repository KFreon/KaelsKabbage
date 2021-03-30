---
title: "26 Azure Perf Investigation"
date: 2021-03-24T11:48:16+10:00
draft: true
type: "post"
slug: "26-azure-perf-investigation"
tags: []
---



<!--more-->  


clients noticed slowdowns randomly, but seemed to affect everyone at once.
did all the normal checks (cpu, ram, etc)
advanced checks like app service diagnostics and troubleshooting SNAT, TCP, etc

view profile traces, EF toarray
Then Framework dependencies, weird... swap context, etc.

add logging to get more info...

colleague download traces, perfview
GC HUIGE!!!
JIT huuuuge!!

x64 vs x86