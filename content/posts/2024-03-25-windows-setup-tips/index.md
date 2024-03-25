---
title: "Windows Setup Gripes"
date: 2024-03-25T16:10:01+10:00
type: "post"
slug: "windows-setup-tips"
tags: ["windows"]
---

I recently setup a new Windows laptop and immediately ran across some issues or things I don't think should be default.  

{{% toc title="TLDR" %}}

<!--more-->  

# Winget is installed, but too old to work  
For some reason, the default Winget is too old to correctly install or update anything including itself.  
I needed to install it "properly" through the Microsoft Store: [App Installer](https://apps.microsoft.com/detail/9nblggh4nns1?rtc=1&hl=en-au&gl=AU#activetab=pivot:overviewtab)  

# UNC shares don't by default  
By this I mean you can't access any network shares on the new machine.  
Turns out this is because if you setup Windows with a Microsoft account, the local account that gets created to support it doesn't get a proper password.  
That means there's no way to authenticate with the share.  Even if you use something like `MicrosoftAccount/ACCOUNT.outlook.com` it doesn't work.  
I didn't bother to fix it this time, but I'm 99% sure the way I fixed it was by ensuring password login was enabled in Windows settings (I may also have had to login using password at least once)  

# Windows sets user folders to Onedrive  
I noticed that the Desktop, Downloads, etc were all pointed to Onedrive.  
On one hand, I can see that it's useful, but I didn't want that because I'm one of those people who dump stuff onto the Desktop and Downloads folder.  

In my case, the main reason I wanted it local was I was setting up a powershell profile and it wasn't letting me set it to Onedrive ¯\\_(ツ)\_/¯  

The fix was going to Onedrive settings --> Backup and unticking everything.  

# Right click --> New, missing .txt?   
There's no option to make new files, only folders.  
I want to be able to make new files, I don't even care what extension.  

- `Regedit --> HKCR/.txt`  
- New Key `ShellNew`  
- New string inside `NullFile`  

These are all mild gripes, but I don't think they're good defaults.  