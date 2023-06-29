---
title: "Gitkraken vs Fork: Facts vs Feelings"
date: 2023-06-27T08:55:10+10:00
draft: true
type: "post"
slug: "gitkraken-vs-fork-facts-vs-feelings"
tags: ["git", "gitkraken", "fork"]
---

I've tried almost all the major git clients out there, and these two are the main ones that stuck with me.  
Tower and Sourcetree just didn't jive with me, Git Extensions...I really hated, even VSCode and Visual Studio git are fine in a pinch.  
I started with Gitkraken, moved to Fork because it was cheaper and I wanted to try something new, so after a year, how do they compare?

<!--more-->  

{{< splitter >}}
{{< split side=left title="[Fork](https://git-fork.com/)" >}}
{{< image path="img/ForkUI" alt="Fork is a fairly simple UI, nothing flashy" >}}
{{< /split >}}
{{< split side=right title="[Gitkraken](https://www.gitkraken.com/)" >}}
{{< image path="img/GitkrakenUI" alt="Gitkraken's UI is possibly simpler, but has some fancier looks" >}}
{{< /split >}}
{{< /splitter >}} 

{{< splitter >}}
{{% split side=left title="[Fork](https://git-fork.com/)" %}}
## Facts
- $50 one-off
- Windows, Mac

## Pros
- Fast  
- Closer to Git (shows actual git results and info etc)
- Easy access to all the common things
- Good keyboard support
- Uses GCM  

## Cons
- Not super flashy
- Can't set defaults for things like pull rebase, force push, etc
{{% /split %}}
{{% split side=right title="[Gitkraken](https://www.gitkraken.com/)" %}}
## Facts
- $60/month  
- Windows, Mac, Linux

## Pros
- Flashy
- Loads of integrations built in
- Nice defaults and helpful suggestions (do you want to force push?)
- Sexy git graph

## Cons
- Slower than Fork
- No GCM ☹️
- Yearly subscription instead of one-off payment  
{{% /split %}}
{{< /splitter >}}  

The differences aren't significant between Gitkraken and Fork, but something FEELS different.  
Even now writing this, I don't know whether which one feels better... It's almost a mix of both.  

I don't know whether I'll come out of this with a recommendation either way, but definitely both are great.  

Let's take a bit of a look at some facts.  
> **Disclaimer**
> Gitkraken was my first real git client
> Fork is my current client

# Facts  
## Performance
- Fork is faster
- Gitkraken doesn't feel too slow though

## General Usage
- Gitkraken has some sensible defaults and allows overriding most of them
  - e.g. Pull is a single button click (or right click on branch) and rebases by default  
- Fork is closer to actual Git and asks questions
  - e.g. Pull button brings up a modal asking what remote, whether to rebase etc
- Push is another example. If upstream is ahead of local:
  - Gitkraken will try, then ask whether you want to force push
  - Fork will fail with the git output, and you should have ticked "Force Push" when pushing  
- Git Graph is fairly similar
  - Gitkraken has branches/tags pulled out on the left, hides commit time a bit  
  - Fork inlines the branch/tag with the commit message and explictly shows SHA and commit time
- Both have some kind of profile support, separating the git credentials and behaviours
- Both have tab support across the top  
- Both have commit/graph filtering based on branch or search text  


## Diff  
- Gitkraken has some basic syntax highlighting and shows the entire file by default (I think)
- Fork can show the entire file if requested (not default)
- Both have image support  

{{< splitter >}}
{{< split side=left title="Fork Diff" >}}
{{< image path="img/ForkDiff" alt="Fork has a clean simple diff in the Git vein, nothing flashy" >}}
{{< /split >}}
{{< split side=right title="Gitkraken Diff" >}}
{{< image path="img/GitkrakenDiff" alt="Gitkrakens diff has some syntax highlighting, but s" >}}
{{< /split >}}
{{< /splitter >}}  

## Working with Commits and branches
- Gitkraken can reword commits just by clicking the commit message
- Fork requires interactive rebase
- Fork can do `Ctrl + Enter` in the commit message to commit staged and `Ctrl + Shift + Enter` to commit and push  
- Fork has `Ctrl + B` on a branch to create a new branch  
- Fork stash asks for name when stashing and add asks if you want to delete it when applying
- Gitkraken just one-button does stash and pop, and requires manual renaming and applying without deleting  
- Neither support [git worktrees](https://git-scm.com/docs/git-worktree)  

## Integrations  
- Fork uses the Git Credential Manager, sharing credentials with most other git tools  
- Gitkraken does not support GCM and uses it's own mechanisms (oauth, PAT, etc)  
- Fork can get information about Github PR's, nothing from Azure Devops  
- Gitkraken has a bunch of full integrations, exposing PR's, Issues, and a load of other things

## Other features
- Gitkraken has it's own Workspaces feature allowing a custom board and whatnot for teams, etc
- Gitkraken has a custom terminal with additional git features  
- Fork allows opening the system default terminal
- Fork has a button that allows opening any detected sln in Visual Studio, the whole repo in VSCode, and the Azure Devops url in the default browser

# Feelings  
It kinda feels like I was leaning towards Gitkraken up there, and to some extent, I am.  
It's defaults fit my workflow quite well, and SOMETHING about it just feels a bit nicer.  
Perhaps it's the fact that the defaults and flows mean I get less prompts, and if I set something that ISN'T a default, it's NOT remembered.  
That sounds bad, but recently in Fork, I ticked "Remove stash after applying" and moved on, but it was remembered the next time, which I didn't want.  
That said...Gitkraken auto-deletes it, so I don't know what to make of it.  

Two massive, almost overwhelmingly superior, features Fork has are:
- Performance  
- GCM support  

While Gitkraken isn't really slow, but it's slower than Fork.  
Gitkraken's auth flow is just a barrier too far.  
Fork just pops the standard Git auth window. Gitkraken DOES have something similar, but it's not working as expected for me, and I used to use PAT's, but they only live for 90 days for each organisation (90 days is an org restriction, and I work across multiple Azure orgs and tenants)  

# Conclusion?  
Before I started this post, I was feeling like Gitkraken probably fit my workflow better, but had the massive auth hole holding it back.  
After writing all this, I think that's cemented for me.  

Being able to do something weird like drop or reorder a single commit without fussing with a rebase, or quickly reword the commit message is just...nice.  

