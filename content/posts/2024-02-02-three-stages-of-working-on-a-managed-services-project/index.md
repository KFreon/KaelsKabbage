---
title: "Three stages of working on a Managed Services Project"
date: 2024-02-02T14:39:53+10:00
draft: true
type: "post"
slug: "three-stages-of-working-on-a-managed-services-project"
tags: ["managed-services"]
---

NOT DOING THIS RIGHT NOW, just feels weird.
I'm kinda complaining about how bad I am at my job.



I've been working in Managed Services/Sustained Engineering/etc for over five years now and I've noticed a pattern with our type of projects, or perhaps more accurately, the way they have to be run.  
The overwhelming stage, the comfort stage, and the paralysed stage.  

<!--more-->  

# What is Managed Services?  
For who don't know, or just to get on the same page, Managed Services is where we provide ongoing support to for a client + codebase combination, usually limited hours and cost.  
These projects are in Production and we provide ongoing maintenance, bugfixes, and small features.   
As it's been in Production with users and may have been running for a while, there are often expectations on the turnaround time of tasks, stability of the solution, and communication styles the client is used to. 
Each client tends to get a day or two a week during normal operations, which means limited time to handle comms, do coding, and attend meetings.  

As such, I believe there are three stages to working on a Managed Services project as an individual. 

> This is 1000% an opinion piece, and it feels like I'm telling on myself a bit as well...  

# Stage 1: The overwhelming stage  
You've been given a codebase and client, and you often know little about either, even after a [handover](/content/posts/2022-07-12-managed-services-handovers-the-good-the-bad-and-the-rest/index.md)  
Broadly, you don't know where anything is, how anything works, or why anything is there.  

This tends to mean simple things like text changes or colours are fine, but anything requiring understanding is difficult and time consuming.  
I mentioned communication styles above and I think it's a really important part to think about.  

> Some clients REALLY don't like their devs changing and will make your life even more difficult in this period.  
> You'll often hear "go ask `<previous dev>`" or "look, I just need this now, make it happen"  
> I get where they're coming from, but a good [handover](/content/posts/2022-07-12-managed-services-handovers-the-good-the-bad-and-the-rest/index.md) can help, and aside from that, devs change sometimes ¯\\_(ツ)_/¯.  

# Stage 2: The comfort stage  
You're now familiar with the codebase, the client, and their business (and how the solution fits within it)  
Generally investigations are still required for most things, but you've heard of most of it and can talk to the client reasonably about the processes and potential solutions.  

> Fortunately, this is the largest and longest lasting stage

# Stage 3: The paralysed stage  

You know too much about the codebase, but at the same time not enough and lack the time to investigate.  
The client asks you to make a logic change here, but you know that part is PROBABLY linked with a few others.
You make changes knowing you'll have to come back to it later, littering the codebase with `//TODOs`.  

You're paralysed by knowledge, but lack the time to progress to full understanding.  
One or two days a week gets you context switching frequently and you forget things and confuse different clients, codebases, etc.
This is an indication you need to think about stopping to write/update some documentation and consider moving on or taking a longer break. 

# Wait...what was that last one?  
I was surprised to find that the more I knew about the codebase, the less confident I was in making changes and the more frustrated I'd get about how tangled things were getting.  
That says something about my coding and management style perhaps, but I found that the time and context switching pressures tended towards less maintainable code and thus I'd take a long time to make decisions, which tended to make the time pressures worse.  

For example, I was working on a third party integration which required full stack changes.  
I had a big list of things that needed updating in all the sections I was touching, but I knew that if I did them, it'd triple the work time and require full regression.  
If I was still in Stage 2, I could perhaps have just make the change now in the best way I could, but I have trouble getting the tech debt out of my head.  

It sounds like I'm advocating to just YOLO changes without bothering to understand how they affect other areas of the code, but it's more that with limited time comes limited ability to hold all the context in your head, and that eventually you end up with too much context and not enough space to work.

Some examples:
- **Code duplication**: If it's once or twice, don't fuss too much about it, just do it and fix it if it becomes a problem later
- **Different styles of doing X** (like background workers or PDF generation etc): Controversial option but if you don't really understand how to setup X the way the previous person did it, but you know how to do it another way, I say do it. Especially if yours is based on newer tech or information.
- **Dependencies**: You might remember that three years ago the third party was really slow to respond to requests, so in your current work, you spend time adding all this caching and whatnot to ameliorate that when that might not be a valid concern anymore.

# What can I do?  

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