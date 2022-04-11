---
title: "Upgrading .Net 4.8 to NET 6 - Aspnetcore + Background Service"
date: 2022-03-05T09:19:40+10:00
draft: true
type: "post"
slug: "upgrading-net48-to-net6-aspnetcore-service"
tags: ["net6"]
---



<!--more-->  

details about the upgrade and issues therein
- swapping autofac with the built in
- swapping serilog with the built in
- single file Program.cs experience
- EFCore?
- Testing
  - Integration testing specifically
- auth took so long :(
  - Some custom auth that needed adjusting, but just getting it set back up
- Backgtround service
- signalr

# TLDR
- Auth hard
- NET 6 good
- Windows Service with net 6 good
- Loads of code deleted, good

# Setup  
- AspNet MVC
  - Serilog
  - Entity Framework 6
  - Autofac  
  - Signalr (ancillary use, not core functionality)
  - Custom cookie based authentication
- Windows Service (Topshelf in .Net 4.8)
- Azure build/deploy
- Running on premises

# Desired Outcome
- Aspnetcore
  - Built in logging
  - EFCore
  - MS DI
  - Signalr
  - Custom cookie based auth (still required, just needs to work)
- Windows Service built from aspnetcore hosted service (no Topshelf)
- Azure build/deploy
- Still on premises :(

## Reasoning and thoughts
I wanted to find out what a near full Microsoft solution looked like.  
I make no statements on the efficacy or usability of the third party tools objectively, although I must say that subjectively, the MS DI is more understandable and usable than Autofac...😬  

# Planned Upgrade Path  
Use the [Microsoft Upgrade tool]()! 

# Actual Upgrade Path  
I started off rebuilding the projects to use the dotnetcore csproj structure and target Net 6.  
Making a new project and then copying the relevant bits into it in VSCode/text editor seemed the simplest way, and went pretty smoothly.  

## Nugets
This was a bit more painful, as I needed to find compatible nuget packages. In most cases, that was just "use the latest version", however some put up more of a fight.  

Most upgraded easily enough, perhaps some minor code and namespace changes, however some of the Microsoft items required new packages from when they split one package into many or when the Windows specific functionality was pulled into their own packages
EXAMPLES

## Logging
Simple, still look pretty nice without serilog.
EXAMPLES

## EFCore  
I've written about this [before]() when upgrading to dotnetcore, but the EFCore migration was fairly simple.  
Loads of namespace changes, some extra unexpected packages (hello Microsoft.Data.SqlClient vs System.Data.SqlClient).  
In general, I liked the process, and I liked the new functionality FOR EXAMPLE

## Signlar 
Almost trivial to setup EXAMPLE

## Authentication
The hardest part was, as expected, the authentication. Not necessarily because I had to make so many changes, but because it's so opaque and hard to understand the documentation.  
There's a lot of "I thought this should work, oh it doesn't even though the docs describe ALMOST this scenario, not quite".
My difficulties were exacerbated by my not especially great understanding of authentication and authorisation in the first place, but the docs didn't help as much as expected.  
EXAMPES

### The big mad
How does "default" mean "the base that all others will build off"? EXAMPLE

## Windows Service without Topshelf 
Use this nuget, change the pipeline for service registration and starting.