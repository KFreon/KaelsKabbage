---
title: "Managed Services Handovers: the Good, the Bad, and the rest"
date: 2022-07-24T10:04:45+10:00
type: "post"
slug: "managed-services-handovers-the-good-the-bad-and-the-rest"
tags: []
---

I work in Managed Services, and we often get handover from the Professional Services when they roll off a project.  
What makes a good handover to Managed Services/Sustained Engineering?  
What do we want? What makes us sad?

<!--more-->  

Discussions on Managed Services vs Professional Services tend to focus on greenfields vs brown, handling multiple clients in a week, working with an unfamiliar codebase, etc.
But what can PS do to make our lives easier in MS during the project or during handover?
What docs/info do we want? What do we NOT want to see?  

> This is all my personal opinion. The purpose here is not to judge, but to highlight what I want and what I *think* would be good for others as well. 

{{% toc levels="three" %}}

# First, some context
I have multiple clients at once (*currently 5*).  
That means there's a decent amount of time between time on a particular client.  
This makes deep investigations into issues or behaviours difficult, as context is lost and allows time for more important bugs to build up in that time.  

Anything I can be given that makes these easier is desirable. 

Regardless of how good the handover is, I'll always end up with a big list of notes in Notepad/Onenote/Obsidian for tips based on the context that I have (professional experience, knowledge, project info, etc) like "need to have Redis installed first" or "those settings are over there under the bush behind a fire".

I don't expect to be able to replace the notes, I just hope to reduce mental load and increase uptake of knowledge

# I'm in Professional Services. What can I do?
## ✔️ Good Practices, keep doing
### **Sexy `ResetTheWorld.ps1`-style scripts**  
> These are something you can run after cloning the repo that stands up anything and everything needed to F5 the solution. It can also be run at any time to reset the solution back to a fresh state. 
> Ideally: Create and seed databases, setup ACL entries if required, if builds require custom scripts, RUN THEM.
- Not only does it get us up and running easily, but it shows us what it takes to set up this project (if we move build systems or up to the cloud or something)

> The remainder of these assume the above **DOESN'T** exist

### **A Code style setup** (EditorConfig/Prettier)
- Ideally enforced in build 
- Needs IDE support and/or auto-suggested extensions for VSCode (or at least documented)
- Makes it easier to find things when we haven't seen them before, but we can guess based on code we have seen.
- Consistent style helps cognitive and visual comprehension.

### **A sweet suite of tests**
- Stability is critical. If they're unstable, they're almost worthless and they're gonna get commented out or deleted.  
- I recently had a nice set of visual regression tests, which was gave us confidence, BUT these tests also ended up being flakey and are now just a guide. Which is better than nothing, but will become more useless with age.  
  - They're currently just a warning in the build, which is supposed to prompt us to go look and confirm we haven't broken it, but it's difficult to check in Azure Devops, so we don't and we had legitimately broken tests for a week before we checked...

### **Seed Data**
- Seed data for database or database snapshot
- Also acceptable is a script to run that generates a database
- Getting started without this is difficult and time consuming, usually requiring decent understanding of the codebase that I may not have yet, and can highlight to clients that the handover didn't do enough

### **Well documented build pipeline yml** OR **Documentation around the pipeline**
- Useful to understand how the build works
- Useful to understand how things link together
- Ideally, also useful to indicate HOW and FROM WHERE variables come from
> Recently I spent nearly an hour looking between the build script, Azure, and AWS looking for where this variable was coming from. In the end, it was being set invisibly in the build step, BUT I did learn that some variables come from Azure Pipelines and some come from AWS Parameter Store.
> Very useful to know.  

### **Infrastructure and Business logic Diagrams/Documentation**  
- Great for understanding how services are connected, most importantly third party services
- Good as reference when viewing code, allowing mapping between infrastructure and code to logical business processes
- Good quick reference for how the client names things and how WE think they're connected

### **Explicit versions**  
- Node versions, cli versions, etc
- Having it in the build script is better than nothing, but if you have a readme, put all the local tech requirements at the top so they can be assumed installed for the remainder of the readme

### **Mobile or Simulator setup** AND **Cross platform instructions**  
- Windows/Linux/MacOS quirks
  - It's a hard one, but it'd be good to have a list of these if someone ran into them, or maybe an entry to indicate that the Readme assumes Windows/Linux/MacOS so I know it won't necessarily "just work"
- Simulator setup can also be critical if there's a flag you need to flick in XCode to make the build work for Cordova projects, etc


### **Other**  
- Instructions on testing 3rd party integrations
  - Postman suites also good
- Who manages non-cloud machines and processes? Do we deploy? Do we just email someone and push the build? Is there a release/change management process? 


## ❌ Bad Practices, consider not doing
### **Workarounds/hacks**
  - **DON'T**, or at least document it especially the WHY does this need to be a hack (time, security, difficulty, etc)
  - Personal client app registrations and secret, magic string to bypass auth in the Dev environment, `appsettings.local.json` with no example or setup instructions on what **SHOULD** be in there for sensible local dev, even a local database snapshot that makes dev usable.  
  - Not having reasons makes it difficult to be confident during a change  

### **Magic strings/tokens**
  - Similar to above, but these are more deliberate and permanent  
  - Things like magic header for localhost that disables auth or a guid that disables certain tests in certain environments  
    - There was a `VSS_FEED_ACCESS_TOKEN` in a `build.yaml` that was never set anywhere, and it turned out it was magically set by an Azure build step. Cool, but I didn't know that, and I spent ages looking for where that was set. A comment could save hours of reading and investigation.
    > If it took you a while to figure out, note it down somewhere so the next person to look doesn't have to do the same.

### **NOT having a local override for dev settings**
  - We ALWAYS want to override settings locally for auth or connection strings
  - **DO NOT COMMIT IT!!**
    - I've seen this done accidentally and it just makes things confusing, because then there's discussions like "should we take it out? What if one of the environments is using it?" 
  - **Explicitly** add to gitignore and have instructions or an example of such an override file in the documentation

### **Utilising uncommon things without documentation**
  > If it took you a while to figure out, note it down somewhere
  - I'm not familiar with AWS yet, so when I gtt an AWS gig, and there were variables and an Azure build script, I assumed it'd be replaced in Azure. Turned out SOME variables were replaced in Azure to new variables which were then pulled from AWS secret store (which incidently, very difficult to find), and some were pulled directly from that store.

### **Logging, how does it work?**
  - Passwords, common log search queries, setup within the tools with log groups/filters
    - Related to above, I don't know how to drive AWS logs well, so I spent a while looking for which of the 20 log groups I was supposed to be looking at (after finding out that Log Groups was what I wanted), then what query I needed to write to find Errors, etc.

---  

One interesting example was around naming, and there was an entity called `unedited <things>`, which sounds like it should mean the thing wasn't edited.  
While technically true, it wasn't quite how it sounded (it created a copy which was the new version, and thus considered "unedited")  
THAT is the kind of thing that needs a comment.  
It might have been correct domain language, but it was confusing to me as an incoming outsider.



# Ultimate super excellent uber goal
`docker compose up`

Full dev environment in Docker.  
No local installations or setup except docker.  
All development in Docker. All running in Docker.  
D O C K E R

I believe things are heading that way, and that is quite exciting, but we do need to set our future selves and colleagues up for that success.  
Also...  

> If there was a discussion or decent research required in the team, write it down for people later!
