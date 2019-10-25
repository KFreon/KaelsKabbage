---
title: "Adventures in Xamarin Forms"
date: 2018-07-30T11:11:59+10:00
draft: false
type: "post"
slug: "adventures-in-xamarin"
tags: ["xamarin"]
---

### Issues getting started, and differences to WPF/UWP

Xamarin Forms should be an easy sell.

- Work in C# and XAML. Two good (XAML...?) and well defined languages  
- Works across major mobile OS's  
- Even works for UWP, enabling a desktop experience if desired  

But there's a shadow that hangs over Xamarin, one shared by most mobile dev platforms: **FIDDLINESS**.  

<!--more-->  

Getting everything working can be curiously painful. Full disclosure, I'm talking about the following setup:

- Xamarin Forms  
- New project --> Mobile App  
- .NET Standard  
- Testing on UWP and Android (Not even going to poke that bear just yet)    

# Issues
### Android
#### Path Length when building
This seems to be an Android code generation thing, as I've seen it in Cordova projects as well.  
In Cordova, the fix is to point at a temporary build directory where everything can be built with short paths.  
In Xamarin, the same options exist.  
Can shorten folder names under VS's control i.e. Project folder and subfolders:
``` xml
<PropertyGroup>
	 <UseShortFileNames>True</UseShortFileNames> 
</PropertyGroup>
```  

Chances are this won't help, so the main option is: *NOTE: Must end in slash*
``` xml
<PropertyGroup> 
	<IntermediateOutputPath>C:\Projects\MyApp\</IntermediateOutputPath> 
</PropertyGroup>
```

It should be noted that these options didn't work for me. They definitely solved this particular problem but created numerous and seeminly un-replicatable problems.
<br>

#### Incorrect architectures and Simulation
I've included these for completeness, but I've gone through them in [another post](https://www.kaels-kabbage.com/post/xamarin-and-hyper-v/) (at the bottom).
The biggest bit out of those is that the build architecture defaults were incorrect, and the simulator needs to be started prior to running the first time (it takes a while to start up)
<br>  

### More General issues
### Versions and upgrades
Upgrading Nuget packages and getting running again is painful. Xamarin Forms nuget packages are tightly bound to some of the other Microsoft packages, and while those packages are also getting updated, it doesn't realise. You can probably force it, or go Prerelease (must have been a dependency somewhere), but then I couldn't get anything running again due to depencency issues across the board. This also seems to be due to .NET Standard support which leads us to...
<br>

#### UWP .NET.Runtime.2.1-rel Error
[It seems](https://doumer.me/how-to-resolve-microsoft-net-native-runtime-2-1-rel-not-found/) that this relates to the Microsoft Store not supporting .NET 2.1 yet, and occured after upgrading some Nuget Packages to a Pre-release version (to try to get around the above issues). Solution was to run as Debug (non-store related configuration). As aluded to above, I was still unable to get things running, but this wasn't the reason.
<br>

#### UWP doesn't hit breakpoints in Debug Mode
Debug mode, why this betrayal? Turns out it's another case of bad defaults.
{{< image path="img/UWPDebugMode" alt="Setting for UWP breakpoints" >}}


# Differences to WPF/UWP
## XAML Controls
- **Generally a limited set of the features of WPF/UWP**
- Views are things that can be viewed, Layouts are things that do layouts, Pages are...well pages, and Cells are things inside tables.  
- The `BoxView` is a strangely named control, used for squares and rectangles, and contains no content. In WPF, it would be `Rectangle`... 
- Still XAML staples such as `Grid` and `StackLayout` (renamed from `StackPanel`), but the different controls are a bit interesting to get used to
- Controls tend not to have the `content` attribute. On reflection, it's likely because there's fewer _general_ components. Instead of having a `Label`  and a `TextBlock`, one essentially an implementation of the other, there's just `Label`.  
- Being quite familiar with WPF, UWP was easier to get familiar with compared to Xamarin (fewer direct parallels)
- Instead of `Context` it's `BindingContext`  