---
title: "Xamarin and Hyper V"
date: 2018-07-10T16:08:35+10:00
draft: false
slug: "xamarin-and-hyper-v"
---

Generally, the Android emulator for Windows ran on Intel's HAXM hypervisor. Unfortunately HAXM runs in the same way as Microsofts Hyper-V, and both cannot be enabled at once. While some users get away without using Virtual Machines (I'm one), many prefer the separation VM's afford, and there's no way they'll ditch Hyper-V capabilities (if that's the hypervisor they're using...). The hardest hit would have to be the Docker users though, as Docker runs it's VM's on Hyper-V (Mine is at least).  
Android [recently updated](https://android-developers.googleblog.com/2018/07/android-emulator-amd-processor-hyper-v.html) their Android Emulator to support Hyper-V AND HAXM, which is great for us! It should be noted that they still tout HAXM as the preferred method, as it's "more performant".  

The [Visual Studio Blog](https://blogs.msdn.microsoft.com/visualstudio/2018/05/08/hyper-v-android-emulator-support/) indicates a preview version of Xamarin and Visual Studio is required, but I got on fine with my standard 15.7.4 install.  
# Method
### Enable new Windows Hypervisor features (Requires April 2018 Update)

{{% image path="/img/XamarinHyperV/WindowsFeaturesHyperV" alt="Enable Hypervisor" %}}
### Update the Android Emulator to 27.8.3+ via SDKManager (Android Studio, Visual Studio, or command line) 
{{% image path="/img/XamarinHyperV/EmulatorUpdate" alt="Update Android Emulator" %}}
### Create/run an Android Virtual Device (AVD) and it'll pick up the new acceleration!
{{% image path="/img/XamarinHyperV/AndroidEmulatorScreen" alt="Run Emulator" %}}
### Run project through Visual Studio AFTER starting the AVD and once it's finished booting (You'll get Package Manager access errors otherwise)
{{% image path="/img/XamarinHyperV/AndroidEmulatorXamarin" alt="Run Xamarin" %}}

Debugging looks to work fine using this method as well, but the best part is just being able to run the app without weird emulation issues or disabling Hyper-V!  

<br></br>  

EDIT: Something else that came up was that some of the build configurations failed to deploy because `INSTALL_FAILED_NO_MATCHING_ABIS`.  

[Turns out](https://stackoverflow.com/questions/24572052/install-failed-no-matching-abis-when-install-apk) this was due to a missing checkbox that seems to be enabled for `debug` but not for most of the other default configs.  
Unwrapping the link, the fix is:  
{{% image path="/img/XamarinHyperV/Armx86Fix" alt="Arm vs x86 checkboxes" %}}

Another thing I noticed was that sometimes deploying to this emulator (in the setup described here) could take on the order of minutes to get running.