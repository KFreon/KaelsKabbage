---
title: "Visual Studio and VS Code Tips and Tricks"
date: 2019-06-26T18:14:13+10:00
draft: false
type: "post"
slug: "vs-vscode-tips-and-tricks"
tags: ["visual-studio", "vscode"]
---

Visual Studio and the new shiny beast that is Visual Studio Code have many nice features, but sometimes there's a few that aren't on or known about by default (usually because they're experimental or stylistic)  
Some you may know, some you may not.  

**Some that I've found useful:** Logpoints, cursor animations, semantic highlighting, parallel stacks, output window filters, and unreferenced assembly Intellisense.

<!--more-->  

# Visual Studio  
### Logpoints  
Fancy addition to breakpoints. Many people know about the conditional option, but you can also add some other things, like writing to the console and not breaking, looking for a specific thread, or conditionally being hit a set number of times (for weird debugging that happens only at certain execution count...I guess)  

{{< image path="img/VS_BreakpointContextMenu" alt="Breakpoint context menu shows some interesting options" >}}
{{< image path="img/VS_Logpoints" alt="Logpoints can write in-scope variables and text to console without breaking" >}}
{{< image path="img/VS_FilterBreakpoint" alt="Thread filtered, and breaking on a certain hit count" >}}

Another thing I used a logpoint for was when I didn't know which part of a function was slow, and instead of relying on the Visual Studio step timer (works sometimes, but weird in Debug mode), I had a stopwatch start counting at the top, then added logpoints to write out all the timings.  

### Parallel Stacks  
A prettier way of seeing the call stack. Also helps visualising the actual work the application is doing at the time.  

{{< image path="img/VS_ParallelStacks" alt="Parallel stacks show condensed call stacks for all threads" >}}

This is also a good time to bring up the performance tools next to it as well :) 

{{< image path="img/VS_DiagnosticTools" alt="Rough Memory and CPU usage, and breaking can show call timings" >}}

### Output Window Filtering
By default, the Output window spits out all sorts of garbage like thread exits, module loads, etc. Most of the time, no one needs to see that stuff, so you can filter it out!  

{{< image path="img/VS_OutputMessageFiltering" alt="Filter the output window to hide 'thread exited' messages" >}}

### Experimental Intellisense Suggestions  
Something recently added was the ability for Intellisense to suggest items that aren't referenced. Selecting one such suggestion adds the {{< inline "using" >}} statement to the file.  

{{< image path="img/VS_ExternalIntellisenseSuggestion_Example" alt="Intellisense suggested items that aren't currently available in scope" >}}
{{< image path="img/VS_ExternalIntellisenseSuggestions" alt="Visual Studio options showing where to turn on the experiement" >}}


# Visual Studio Code  
I sometimes found it hard to track where the cursor was when I was quickly moving it around, like sometimes I'd scroll and not really notice how far I'd scrolled, etc.  
A bit weird, sure, but there was a solution to my weirdness.  

### Smooth Scroll and Cursor Animations
There's settings for smooth scrolling (can be a bit of a performance hit), and animations for the cursor blink (some crazy ones like pulse and vertical grow), and the transition for the cursor moving to a new location.  
Both of these are found in the standard settings but are too spaced out to put here.

{{% video path="img/VSCode_Cursors" width="1190" height="200" alt="Different VSCode cursor animations" %}}
{{% video path="img/VSCode_CursorTransitions" width="840" height="310" alt="VSCode smooth cursor position transitions" %}}

### Split Terminals  
I don't see enough people using this. It's especially great for [multi-root workspaces](https://code.visualstudio.com/docs/editor/multi-root-workspaces).  

{{< image path="img/VSCode_TerminalSplit" alt="Splitting the terminal helps me keep context without changing to a new screen" >}}


### Semantic Colourisation  
There's an extension called [bracket pair colourizer 2](https://marketplace.visualstudio.com/items?itemName=CoenraadS.bracket-pair-colorizer) which highlights bracket pairs, indented code, etc to make it a bit easier to visually parse code.  
{{< image path="img/BracketPairColouriser" alt="Much nicer semantic colourisation" >}}

{{% info %}}
As of 2019, Visual Studio does this to an extent as well.  
{{% /info %}}  

# BONUS! Azure Data Studio  
Since Azure Data Studio is built on the same engine as VSCode, many of it's tips work here too! I've got cursor animations and smooth scrolling enabled.  

### Grouped Connections and Tab Backgrounds
Connections in ADS can be grouped together in whatever ways you'd like. I've got mine grouped by UAT, Local, and Production since I don't have enough connections per client to warrant grouping by client.  
This is super handy when you open tabs from each connection as there's a setting to highlight the tab background to easily visually distinguish between groups!  

{{< image path="img/ADS_ConnectionGrouping" alt="Coloured connection groups" >}}
{{< image path="img/ADS_TabColour" alt="Editor tab coloured by the connection group it's in" >}}  


There was also a [recent post](https://devblogs.microsoft.com/visualstudio/visual-studio-tips-and-tricks/) from Visual Studio themselves.  
Enjoy! 