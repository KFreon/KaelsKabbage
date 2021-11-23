---
title: "SwiftUI vs MAUI Initial Experience"
date: 2021-11-23T16:13:30+10:00
type: "post"
draft: true
slug: "swiftui-vs-maui-initial-experience"
tags: ["iOS", "maui", "xamarin"]
---

# SwiftUI 
- seemed fine, like WPF.
It's their way or the highway.
I like the preview capabilities, multiple different ones displayed at once for small components.

# MAIU
OOTB...wouldn't build (missing/incorrect Android SDK setup)
- Once that was fixed, wouldn't start the emulator.
  - Maybe it's cos I already had Android studio setup?
- Manually start emulator via Android studio, shows up as local device, build!
  - Default configuration was to NOT deploy... Maybe it was cos it was set to Debug? Still weird.
- Setup to deploy, deploys nicely.
--> OVERALL I'd say if I didn't have Android studio, it'd be fine.
- Also I needed the Universal WIndows platform workload installed, which I thought was unexpected. I suppose MAUI can run on Windows too.

MAUI Check tool 100% necessary. `dotnet tool install -g Redth.Net.Maui.Check`
Then `maui-check` 
It'll fix everything.
Ok...MAUI just isn't ready yet.
It seems to work and all, but no designer previews or anything. Pretty hard 

Pretty cool, MAUI can be built like SwiftUI. i.e. fluent building style