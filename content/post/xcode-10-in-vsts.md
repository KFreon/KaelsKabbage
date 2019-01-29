---
title: "Forcing any version of Xcode on VSTS Mac build agent"
date: 2019-01-29T13:45:59+10:00
draft: false
type: "post"
---

Developers using the Apple Store might have recently noticed a message indiciating that "This app has been built for iOS 11.x. Starting March 2019, all apps will be required to target iOS 12.1 available in Xcode 10.1"  

**DISCLAIMER:** We haven't yet had the opportunity to submit to the store to see if it accepts it, but the builds now work targetting the higher version.

Normally this is fine, just upgrade Xcode and rebuild. Not so obvious for VSTS (ADOPS?) and Cordova.  
Surely Microsoft would just have the latest installed right?  
Hmm, well kind of. If you look at the Mac build agent details, there's versions of ALL major Xcode builds, it's just that 8.x is set as default.
{{% image path="/img/Xcode-10-in-vsts/VSTSAgentXcodeVersions" alt="The many versions of Xcode supported by VSTS Mac build agent" %}}  

Hmm, well maybe there's a build step to set the default version of Xcode for this session?  
Not that I could find.  

We need to set the version for Cordova, but Cordova doesn't provide an obvious way of setting the desired Xcode version, and there's a caveat in the way they do provide.
Turns out (after a bunch of googling) that you can set the path where Xcode is found.  

- Not sure about the cmdline.  

Below shows the area in the VSTS Cordova build step to add. Remember that the VSTS Mac build agent has all lots of versions, and changing the version number seems to correctly target the version specified.  
{{% image path="/img/Xcode-10-in-vsts/XcodeDevPath" alt="Cordova build step, iOS, Xcode developer path" %}}  

Yay, that was easy!  

*Build Failed with some cryptic error*  

Oh.  

Turns out that Xcode has updated their build structure, since version 8 and thus there's a build flag to use the older system, as shown below.  
{{% image path="/img/Xcode-10-in-vsts/XcodeBuildFlags" alt="Cordova currently requires that Xcode use the UseModernBuildSystem be set to 0" %}}   

Builds should succeed!  
Again, we're yet to test these builds actually get to the stores, but at least there's learnings here :)