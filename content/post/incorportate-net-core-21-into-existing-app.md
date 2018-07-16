---
title: "Incorporating .NET Core 2.1 into existing .NET Framework/UWP applications"
date: 2018-07-09T10:47:07+10:00
draft: false
type: "post"
---

EDIT: As a colleague pointed out, the method below could potentially cause errors at runtime if the [.NET Core 2.1 runtime](https://www.microsoft.com/net/download/dotnet-core/runtime-2.1.0) is not installed.  


<br></br>  
 

The recently announced [.NET Core 2.1](https://blogs.msdn.microsoft.com/dotnet/2018/05/30/announcing-net-core-2-1/) brings some nice new features, but the big draw is the support of [Span<T>](https://docs.microsoft.com/en-us/dotnet/api/system.span-1?view=netcore-2.1) and it’s performance happy friends.

There’s a slight catch though. .NET Core 2.1 projects can’t be referenced by UWP and .NET Framework projects, so if you have a desktop application in need of a (potential) performance boost to, it’s not as easy as it could be.

{{% image path="/img/IncorporateNetCore21/UWP_Reference_Error" alt="UWP Reference Error" %}}
<p class="subtitle">Reference Errors</p>

That’s a bummer.

For those who really want to use Span<T> and the other constructs from .NET Core (without having to use the System.Memory [nuget package](https://www.nuget.org/packages/System.Memory/)) in their non-Core frameworks, there are some workarounds.

One is to wrap up the .NET Core project as a Nuget package, however the development feedback loop is quite steep. If the project is standalone, this could be a suitable solution, but not so much if the project is just logically separated.

The most suitable solution investigated was to directly reference the .dll (instead of the project itself), although requiring a bit more setup than usual. Referencing the .dll doesn’t build the project it comes from, so if changes aren’t reflected until the project is manually rebuilt.

{{% image path="/img/IncorporateNetCore21/Direct_Reference_DLL" alt="DLL Reference" %}}
<p class="subtitle">Directly referencing the generated DLL (requires initial build to generate it)</p>  

This can be alleviated by setting the Core project as a dependency of the non-core project, causing it to be built prior.

_Right click Solution –> Properties_

{{% image path="/img/IncorporateNetCore21/Dependencies" alt="Dependencies" %}}
<p class="subtitle">Dependent projects are built first</p>
This leads to the usual F5 experience!

The caveat here is that the configuration doesn’t adapt to the configuration. There is apparently [a way of conditionally referencing different dlls](https://stackoverflow.com/questions/1786917/is-there-a-way-to-specify-assembly-references-based-on-build-configuration-in-vis/1787075#1787075) to fit configurations, however this might be excessive for most use cases.

 

{{% image path="/img/IncorporateNetCore21/Configuration_Manager" alt="Configuration Manager" %}}
<p class="subtitle">Build configuration can specify different project build types</p>
 

This is only required as long as the tooling doesn’t support these combinations, however this method should continue to work for the cutting edge versions.

Enjoy the new toys!