---
title: "Managed Services Handovers: the Good, the Bad, and the rest"
date: 2022-07-12T19:04:45+10:00
draft: true
type: "post"
slug: "managed-services-handovers-the-good-the-bad-and-the-rest"
tags: []
---

I work in Managed Services at Telstra Purple, and we often get handover from the Professional Services when they roll off a project.  
What makes a good handover to Managed Services/Sustained Engineering?  What do we want, what do we care about, what do we cry about when we can't find it?

<!--more-->  

Posts on Managed Services vs Professional Services tend to focus on greenfields vs brown, handling multiple clients in a week, working with an unfamiliar codebase, etc.
But, sometimes we get a direct handover from the PS team rolling off, so what can they do to make our lives easier in MS?
What docs/info do we want? How would we like the code to be like?

> This is all my personal opinion. The purpose is not to judge, but to highlight what I want and what I *think* would be good for others as well. 

# First things first
I have multiple clients at once (*currently 5*).  
That means there's a decent amount of time between time on a particular client.  
This makes deep investigations into issues or behaviours difficult, as context is lost and allows time for more important bugs to build up in that time.  

Anything I can be given that makes these easier is desirable. 

Regardless of how good the handover is, I'll always end up with a big list of notes in Notepad/Onenote/Obsidian for tips based on the context that I have (professional experience, knowledge, project info, etc) like "need to have Redis installed first" or "those settings are over there under the bush behind a fire".

I don't expect to be able to replace the notes, I just hope to reduce mental load and increase uptake of knowledge

# I'm in Professional Services. What can I do?
PS are rarely in our shoes.  
You have deadlines, timeframes, demos, etc, and perhaps critically, it's usually greenfields or new enough that everyone who wrote it is still around, and likely back on the project for Phase 2.
We don't get that. 
When we find something crazy, we usually complain in `#tantrums` or `#fml` then write a good note all over it because it wasted our time.

So as a Professional Services developer, what can you do to help us?

<br/>
<br/>
<br/>
<br/>
DO I WANT THESE CARD THINGS? If so, click anywhere should expand/collapse
Thoughts, I like the look, but if it's already a list, why not expand them all?
Maybe it should show a bit of context with a click to fully expand?
<br/>
<br/>
<br/>
<br/>

## ✔️ Good Practices, keep doing
{{< collapsible title="**A Code style setup** (EditorConfig/Prettier)" isBox=1 >}}
- Ideally enforced in build 
- Needs IDE support and/or auto-suggested extensions for VSCode (or at least documented)
- Makes it easier to find things when we haven't seen them before, but we can guess based on code we have seen.
- Consistent style helps cognitive and visual comprehension.
{{< /collapsible >}}
{{< collapsible title="**A sweet suite of tests**" isBox=1 >}}
- Stability is critical. If they're unstable, they're almost worthless and they're gonna get commented out or deleted.  
- I recently had a nice set of visual regression tests, which was gave us confidence, BUT these tests also ended up being flakey and are now just a guide. Which is better than nothing, but will become more useless with age.  
  - They're currently just a warning in the build, which is supposed to prompt us to go look and confirm we haven't broken it, but it's difficult to check in Azure Devops, so we don't and we had legitimately broken tests for a week before we checked...
{{< /collapsible >}}
{{< collapsible title="**Sexy `ResetTheWorld.ps1`-style scripts**" isBox=1 >}}
> These are something you can run after cloning the repo that stands up anything and everything needed to F5 the solution. It can also be run at any time to reset the solution back to a fresh state. 
> Ideally: Create and seed databases, setup ACL entries if required, if builds require custom scripts, RUN THEM.
- Not only does it get us up and running easily, but it shows us what it takes to set up this project (if we move build systems or up to the cloud or something)
{{< /collapsible >}}
{{<collapsible title="**Well documented build pipeline yml** OR **Documentation around the pipeline**" isBox=1 >}}
- Useful to understand how the build works
- Useful to understand how things link together
- Ideally, also useful to indicate HOW and FROM WHERE variables come from
> Recently I spent nearly an hour looking between the build script, Azure, and AWS looking for where this variable was coming from. In the end, it was being set invisibly in the build step, BUT I did learn that some variables come from Azure Pipelines and some come from AWS Parameter Store.
> Very useful to know.  
{{< /collapsible >}}
{{< collapsible title="**Infrastructure and Business logic Diagrams/Documentation**" isBox=1 >}}
- Great for understanding how pieces are connected and how the client names things and how WE think they're connected.
{{< /collapsible >}}

## ❌ Bad Practices, consider not doing
- **Workarounds/hacks**
  - Often for Auth or local setup. *e.g. *
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


