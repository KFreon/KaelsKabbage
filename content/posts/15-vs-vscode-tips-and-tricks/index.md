---
title: "Visual Studio and VS Code Tips and Tricks"
date: 2019-06-26T18:14:13+10:00
draft: false
type: "post"
slug: "vs-vscode-tips-and-tricks"
tags: ["visual-studio", "vscode", "productivity"]
---

A colleague mentioned they weren't aware of Logpoints in Visual Studio, so here's some things that aren't on or known about by default (usually because they're experimental or stylistic)  

**Some that I've found useful:**   

- Logpoints  
- Cursor animations  
- Semantic highlighting
- Parallel stacks
- Output window filters
- Unreferenced assembly Intellisense  

<!--more-->  

# Visual Studio  
### Logpoints  
Fancy addition to breakpoints. Many people know about the conditional option, but you can also add some other things, like writing to the console and not breaking, looking for a specific thread, or conditionally being hit a set number of times (for weird debugging that happens only at certain execution count...I guess)  

![Breakpoint context menu shows some interesting options](img/VS_BreakpointContextMenu.png)
![Logpoints can write in-scope variables and text to console without breaking](img/VS_Logpoints.png)
![Thread filtered, and breaking on a certain hit count](img/VS_FilterBreakpoint.png)

Another thing I used a logpoint for was when I didn't know which part of a function was slow, and instead of relying on the Visual Studio step timer (works sometimes, but weird in Debug mode), I had a stopwatch start counting at the top, then added logpoints to write out all the timings.  

### Parallel Stacks  
A prettier way of seeing the call stack. Also helps visualising the actual work the application is doing at the time.  

![Parallel stacks show condensed call stacks for all threads](img/VS_ParallelStacks.png)

This is also a good time to bring up the performance tools next to it as well :) 

![Rough Memory and CPU usage, and breaking can show call timings](img/VS_DiagnosticTools.png)

### Output Window Filtering
By default, the Output window spits out all sorts of garbage like thread exits, module loads, etc. Most of the time, no one needs to see that stuff, so you can filter it out!  

![Filter the output window to hide 'thread exited' messages](img/VS_OutputMessageFiltering.png)

### Experimental Intellisense Suggestions  
Something recently added was the ability for Intellisense to suggest items that aren't referenced. Selecting one such suggestion adds the `using` statement to the file.  

![Intellisense suggested items that aren't currently available in scope](img/VS_ExternalIntellisenseSuggestion_Example.png)
![Visual Studio options showing where to turn on the experiement](img/VS_ExternalIntellisenseSuggestions.png)

### Keyboard Shortcuts

- `Shift + Enter`: Adds and moves to a new line below the line you're on. This usually correctly terminates a line with a `;` as well.  
- `Shift + Alt + =` and `Shift + Alt + -`: Expands and contracts the selection of the current block. e.g. with your cursor in the body of an `if` statement, `Shift + Alt + =` selects the whole `if` block. Hitting it again, selects the encompassing block, and so forth.  
- `Ctrl + [, S`: Find current file in the Solution Explorer.  
- Multi-caret support is still a thing, with box select `Hold Alt + left mouse` but also with `Hold Alt and Ctrl + click left mouse` so you can place carets anywhere for editing.  
- `Shift + Alt + ;`, `Shift + Alt + .`, `Shift + Alt + ,`: Add cursors to all matches, add cursor to next match, remove cursor from previous match.  
- There's a new pinning feature for the debugger! It behaves similarly to the `DebuggerBrowsableAttribute` but at debug time instead of build. Instead of seeing `{SomeObject}`, you can open its properties and pin one, and see `{Property = val}` instead!


# Visual Studio Code  
I sometimes found it hard to track where the cursor was when I was quickly moving it around, like sometimes I'd scroll and not really notice how far I'd scrolled, etc.  
A bit weird, sure, but there was a solution to my weirdness.  

### Smooth Scroll and Cursor Animations
There's settings for smooth scrolling (can be a bit of a performance hit), and animations for the cursor blink (some crazy ones like pulse and vertical grow), and the transition for the cursor moving to a new location.  
Both of these are found in the standard settings but are too spaced out to put here.

{{< video path="img/VSCode_Cursors" width="1190" height="200" alt="Different VSCode cursor animations" >}}
{{< video path="img/VSCode_CursorTransitions" width="840" height="310" alt="VSCode smooth cursor position transitions" >}}

### Split Terminals  
I don't see enough people using this. It's especially great for [multi-root workspaces](https://code.visualstudio.com/docs/editor/multi-root-workspaces).  

![Splitting the terminal helps me keep context without changing to a new screen](img/VSCode_TerminalSplit.png)


### Semantic Colourisation  
There's an extension called [bracket pair colourizer 2](https://marketplace.visualstudio.com/items?itemName=CoenraadS.bracket-pair-colorizer) which highlights bracket pairs, indented code, etc to make it a bit easier to visually parse code.  
![Much nicer semantic colourisation](img/BracketPairColouriser.png)

> As of 2019, Visual Studio does this to an extent as well.  

# BONUS! Azure Data Studio  
Since Azure Data Studio is built on the same engine as VSCode, many of it's tips work here too! I've got cursor animations and smooth scrolling enabled.  

### Grouped Connections and Tab Backgrounds
Connections in ADS can be grouped together in whatever ways you'd like. I've got mine grouped by UAT, Local, and Production since I don't have enough connections per client to warrant grouping by client.  
This is super handy when you open tabs from each connection as there's a setting to highlight the tab background to easily visually distinguish between groups!  

![Coloured connection groups](img/ADS_ConnectionGrouping.png)
![Editor tab coloured by the connection group it's in](img/ADS_TabColour.png)  


There was also a [recent post](https://devblogs.microsoft.com/visualstudio/visual-studio-tips-and-tricks/) from Visual Studio themselves.  
Enjoy! 