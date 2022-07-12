---
title: "Managed Services: the Good, the Bad, and the rest"
date: 2022-07-12T19:04:45+10:00
draft: true
type: "post"
slug: "managed-services-the-good-the-bad-and-the-rest"
tags: []
---



<!--more-->  
These posts usually cover "How MS is different to PS (multiple clients, a week between each day for a client etc, small changes cos big ones are unweildy), but what about how we actually handle that?
Further, what do we want when we get a PS handover? What docs/info do we want? How would we like the code to be like?


# Multiple Clients
The obvious thing is we have multiple clients to juggle at once, and ages between working on a client.  
Good notetaking is useful, but not 100% required. Generally we bookend the day with emails to properly communicate with the client what we're planning for the day and what we actually did that day.
Reading over those Sent Emails the next time on that client usually provides enough context about what to continue on (if there's no standup or anything formal like that)

Notes are handy, a basic text document list, or something fancier like Onenote or Obsidian for extra goodies.
Makes it nice to manage tips for clients (gotta run this before this, oh those settings are over there under that bush behind a fire), PATs for later use, etc 

# Making that easier from a PS perspective
PS rarely are in our shoes. You have deadlines, timeframes, demos, etc, and perhaps critically, it's usually greenfields or new enough that everyone who wrote it is still around, and likely back on the project for Phase 2.
We don't get that. 
When we find something crazy, we usually complain in #tantrums then write a good note all over it because it wasted our time.

What are some examples of those kinds of things?
Let's make it a Good/Bad list

## Good Practices, keep doing
- Nice code style setup, like EditorConfig and Prettier, makes working with the solution much nicer, as there's a consistent style so when we're looking for something we've never seen, we can more accurately guess what it might be/look like.
- A sweet suite of tests. Don't really care about how many tests, but the quality and depth and stability of the tests.
  - That sounds obvious and standard, but if there's even a hint of "too much effort to even begin understanding them", there's a real possibility that the whole suite gets commented out or disabled in the build, confidence crushed.
  - I recently had a nice set of tests with visual regression tests, which was fantastic cos we end up with good confidence that changes aren't massively breaking things unexpectedly, BUT these tests also ended up being flakey and are now just a guide. Which is better than nothing for sure :)
- Sexy `ResetTheWorld.ps1` scripts
  - Not only does it get us up and running easily, but it gives us an idea and reference for what it takes to set things up (if we move build systems or up to the cloud or something)
- Well documented build pipeline yml
  - This is great for understanding how the build works in environments and stuff as well as where variables come from and whatnot.

## Bad things, consider not doing
- Workarounds, hacks, etc for authentication or local setup
  - Ok if well documented as to WHY as well as HOW
  - Just doing it without explanation usually leads to big investigations when it stops working after a tech upgrade or infra change
- Magic strings/tokens in setup without explanation
  - Seems to be a thing for build pipelines. e.g. there was a VSS_FEED_ACCESS_TOKEN that was never set anywhere, and it turned out it was magically set by an Azure build step. Cool and fine for PS who know about it, but now when I need to change/add/fix that, a comment could save hours of reading and investigation.
- NOT having a local override for dev settings
  - We almost ALWAYS want to override settings locally for auth or connection strings.
  - DO NOT COMMIT IT!! Even better, explicitly add to gitignore
- Utilising uncommon things without documentation
  - AWS isn't common with Purple yet, so when we get an AWS gig, and there's variables and an Azure build script, we're gonna assume it's there. Turned out the variables were replaed in Azure to new variables which were then pulled from AWS secret store (which incidently, very difficult to find)
- HOW DOES THE LOGGING WORK? Where do I go? How do I find things?


# Ultimate super excellent uber goal
Full dev environment in Docker. Nothing local at all except docker. All development in Docker. All running in Docker.
Docker.


node versions, simulator/emulator setup, etc
getting actual data to test with
how to test 3rd party integrations
how does the build/deploy work and who manages it, etc

Would be good to have presence of mind that when the PS team do something they had to research or struggle with, make a note of it in code/readme.
e.g. QPS "unedited variations"
variations already sound like edits
and unedited can refer to something we'd consider an edit
But then  the variable is in Domain language (i.e. Unedited variations) --> SHOULD have a comment on there to explain that.

Handover from PS for QPS Bail
- Had all this filtering and editing of variations etc.
  - I don't care...Don't know how it was before, so indicating changes is irrelevant really.
  - EXCEPT new integrations and build processes, etc
- 