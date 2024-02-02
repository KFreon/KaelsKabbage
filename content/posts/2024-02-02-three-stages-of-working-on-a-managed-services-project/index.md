---
title: "Three stages of working on a Managed Services Project"
date: 2024-02-02T14:39:53+10:00
draft: true
type: "post"
slug: "three-stages-of-working-on-a-managed-services-project"
tags: ["managed-services"]
---

I've been working in Managed Services/Sustained Engineering/etc for over five years now, and I've noticed a pattern with these kinds of projects, or perhaps more accurately, the way they have to be run.  

<!--more-->  

Managed Services is about working on and managing an ongoing client and codebase.  
It's in Production and has users and may have been running for a while, which means there are expectations on the turnaround time of tasks, stability of the solution, and communication styles. 

> COMMS STLYES LIKE client demands things instead of requesting. New devs will just placate and do whatever they need to.
Stage 2+ devs will have more weight and evidence to manage the situation.

As such, I believe there are three stages to working on a Managed Services project as an individual.  

# Stage 1
> The overwhelming stage.  

This is where you get a codebase and client, and you often know little about either, even after a [handover]({{< ref "/posts/2022-07-12-managed-services-handovers-the-good-the-bad-and-the-rest/index.md" >}})  
Broadly, you don't know where anything is, how anything works, or why anything is there.  
This tends to mean simple things like text changes or colours are fine, but anything requiring understanding is difficult and time consuming.  

# Stage 2
> This is the comfort stage.  

You're now familiar with the codebase, the client, and their business (and how the solution fits within it)  
Generally investigations are still required for most things, but you've heard of most of it and can talk to the client reasonably about the processes and potential solutions.

> This is the largest and longest lasting stage

# Stage 3
> This is the other overwhelming stage.  

You know too much about the codebase, but at the same time not enough.
The client asks you to make a logic change here, but you know that part is POSSIBLY linked with a few others.
You make changes knowing you'll have to come back to it later, littering the codebase with `//TODOs`.  
You're paralysed by knowledge, but lack the time to progress to full understanding.  
One or two days a week isn't enough time for that, you forget, you confuse different clients/codebases, etc.
This is an indication you need to think about stopping to write/update some documentation and consider moving on. 

# Wait...what was that last one?  
It was surprising to me as I figured the more you knew about the project, the better, however I realised that I was making (or not making) some decisions based on what I knew (or thought I knew) that was only in my head.
Things like third party integrations or database changes or other upgrades/tech changes.
I knew about tech debt that I wanted to update, but that had to happen before this other integration to be smooth.
And if I did that, we'd probably have to change the auth as well, etc.

If I didn't know about that tech debt (or were more able to see it objectively due to less investment in the project) perhaps I'd just make the change now in the best way I could.
Then later on, when the time was allocated for technical upgrades or whatnot, THEN handle those tech debt issues I was thinking about.

It sounds like I'm advocating to just YOLO changes, don't bother understanding how they affect other things, but it's more that with limited time comes limited ability to make changes.
Make the smallest changes possible to resolve the issue.
Making small changes is hard when you think you know it'll impact or otherwise affect other things.
Some examples:
- **Code duplication**: If it's once or twice, don't fuss too much about it, just do it and fix it if it becomes a problem later
- **Different styles of doing X** (like background workers or PDF generation etc): Contriversial option but if you don't really understand how to setup X the way the previous person did it, but you know how to do it another way, I say do it. Especially if yours is based on newer tech or information.
- **Dependencies**: You might remember that three years ago the third party was really slow to respond to requests, so in your current work, you spend time adding all this caching and whatnot to ameliorate that when that might not be the case anymore, or just might not be needed for this change.

# Tips for prolonging Stage 2 OR reducing the effect of Stage 3
- Update documentation regularly
	- When something comes up from the client and you have to consider it, it's probably worth writing down somewhere
- Keep good notes (personal and on tickets/sharepoint/wherever)
	- Personal is good for keeping track of what YOU want to do for whatever reason
		- "Fix that stupid code I wrote over there" it's not wrong, it's just not how you want it
		- It's also good for why some changes were made when you might not want to put that in official places...
- Add comments for WHY some code exists (or not)
	- This works well with the next point, but if you remove a chunk of code it's sometimes good to note why it was removed
- Use the git commits to explain WHY something was done
	- "Removed feature X" isn't as useful as "Removed feature X because if users' name has a cyrillic letter, it emails the Russian government"
	- This is especially useful when you or the client want to bring that feature back from years ago.
- Ideally keep the codebase up-to-date
	- Conflicts with the issues in Stage 3, but if you can keep things from getting out of hand, they never will right???????
- Ideally have tests for weird things
	- This is ideal and some would say required, but if the test suite you're given doesn't make it easy to add new data or examples, sometimes it just takes too long.
		- "If it takes too long, just fix it" or "If you can't add tests, how do you know it works"?
		- I get it, but if the client says "Prod is down, we need this ASAP, I've tested your fix, make it happen naow" and you're busy on other clients as well tests are often the thing that gets cut.
		- Note it down and try and come back and write tests
		- Sometimes we don't have the pull to push back and say "no, if we want to be sure, we need tests"
		- Sometimes the client goes "I don't care, just fix the thing and get back to the ticket that I also need today"
		- ¯\\_(ツ)_/¯
- When making tickets for yourself, put some detail in them
	- I see lots of bugs or tasks in Azure Devops like "Remove that thing in the spreadsheet from that email"
	- You probably come back to it 5 months later and wonder what you were on about