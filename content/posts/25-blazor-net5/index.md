---
title: "Playing with Blazor in .NET 5"
date: 2020-11-13T14:39:27+10:00
draft: false
type: "post"
slug: "blazor-net5"
tags: ["blazor"]
---

[.NET 5](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) was released recently, with boatloads of goodies.  
Blazor got some attention in this release, and I had some free time to play with it.  
There's good news and bad news...

<!--more-->  

# TL;DR  
- Get scoped SCSS working early  
- Get used to the build/rebuild F5 loop  
- It's almost a good time  

I wanted to make something that COULD potentially be used for something and ended up with this:  
{{% video path="img/TechDebtRadar" alt="Final simple product" %}}  

# What this post is and isn't  
This isn't to detail how to Blazor, or compare between Server and Webassembly implementations, or performance.  
It's just about File --> New Project and spinning up some basic Use Cases and how that felt.  

# Setup  
- Visual Studio 16.8  
- .NET 5 day 1 (no minor versions)  
- Blazor Server (NOT Webassembly, the Signalr update one)  

# File --> New Project: BLAZOR  
The initial setup experience is fairly nice.  
Projects are scaffolded out with useful yet minimal setup, e.g. Some basic components as a reminder of how to do things, Bootstrap included to reduce requirements on custom styling.  

The finished project structure is below, and much of it was there to begin with. I just added some files.  
{{< image path="img/GeneralStructure" alt="Project Structure from template is pretty good" >}}  

# Tooling is...there  
## Not-so-hot reload  
Currently, there's no hot reloading for Blazor Server :(  
There is `dotnet watch` which watches and rebuilds on change, but it's essentially doing a rebuild, so it's a bit slow.  
It also seems to be a bit flakey, missing/skipping changes, but that could be configuration and I didn't explore too much.  
For my simple project, the build/rebuild cycle in VS isn't too bad, but I can imagine in large, slow-to-build projects that this would be unacceptable.  

## Generally flakey  
VS gives up when I rename a `.razor` file, requiring a "turn it off and back on again".  
This also happens every time I add a code-behind file for razor. (This may have been fixed in VS 16.8.2)  
The CSS intellisense is a bit out of date (could be fixable, I haven't dug) along with some of the built in components.  
`InputText` is an `input` set to text along with extra defaults and config, but it's intellisense doesn't support `placeholder`, BUT you can just put it there and it'll work correctly.  

# CSS  
Bootstrap is included by default, which can take a lot of fuss out of the first version or a spike.  
Sharing CSS can probably be done a few ways, but I went with the Sass `@import` directive.  

## Scoped CSS is dope  
`MyComponent.razor` can have a `MyComponent.razor.css` and any styles in there are scoped to the component.  
Is this CSS Modules? I'm not sure.  

## Sassy
SASS compilation is easily supported by things like [WebCompiler](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebCompiler) and [Delegate.SassBuilder](https://github.com/delegateas/Delegate.SassBuilder), anything that hooks in and compiles to CSS before Blazor gets involved.  
I went with `Delegate.SassBuilder` as it's a nuget package that doesn't require IDE setup.  

> `sassc.exe` in `Delegate.SassBuilder` didn't support colour transparency or sass functions and requires [updating](https://github.com/tuhlmann/sassc-binaries/blob/master/v3.6.1/win32/sassc.exe) :/

## Additional Setup Required  
CSS also needed additional setup out of the box.  
In `_Host.cshtml` which manages the main html skeleton, we need `<link href="MyProject.styles.css" rel="stylesheet">` to pull in those scoped modules.  
The modules also need to be set to content, which can be done in csproj using blob patterns:  
``` xml
	<ItemGroup>
		<None Remove="Pages\*.razor.css" />
	  </ItemGroup>

	  <ItemGroup>
		<Content Include="Pages\*.razor.css" />
  </ItemGroup>
```  
> I'm not 100% why we need a `None` AND `Content` there...  

SASS changes sometimes need a project rebuild as well, but not always...  

# Dayta/Dahta  
Databinding is available in Blazor and seems pretty cool.  
It's a bit undiscoverable though.  
Below is a form:  
``` html
<EditForm>
  <DataAnnotationsValidator />
  <InputText ... />
  <ValidationSummary />
</EditForm>
```  
Those "validator" bits are key, and there's no templates or prompts for that.  

## Attributes  
Validation is done through attributes on data models, shown below:  
{{< image path="img/ModelAttributes" alt="Attributes describe that property is required and needs to be at least 10 and at most 50" >}}  
They seem fairly easy to understand and use.  


## Razor though...  
I find the Razor side of databinding a bit less intuitive.  
Some text input examples:  
``` html
<!-- This two way binds the input to the model using OnChange (i.e. only sets when unfocussed), nothing fancy -->
<InputText @bind-value="SomeModelValue" placeholder="Enter some text..." />

<!-- This two way binds the input to the model using OnInput i.e. basically keydown, but also registers keydown to something else -->  

<!-- I used this as a debounced search box. OnInput keeps the model up to date, and onkeydown does debouncing with System.Timer -->
<input @bind="@SomeModelValue" @bind:event="oninput" @onkeydown="OnTextChanged" />
```  
There's also `onchange` and `@onchange`... I don't know what the first one is for, but the second one is the one that takes a delegate.  

# Dependency Injection  
It's not all niggly annoyances though!  
DI is supported in Razor files via `@inject <serviceName> name` and can then be used in the markup.  
I found it most useful for `NavigationManager` which is how you trigger navigations from code.  

# Other random observations  
- `_imports.cshtml` is where common `usings` can live. Data models, injected services, extension methods, etc are good here.  
- The generated html that's sent to the client is fine, if a bit messy (lots of comments and stuff for Blazor)
    - Might be better in release versions?  
- Events and whatnot can be async VERY easily. It's all handled nicely behind the scenes.
    - Extends to the lifecylce events, which is double nice.  
- UI updates only work on the UI thread. `InvokeAsync(() => DoThing())` to marshall.  
- Routing is interesting. It's just a `@page` directive at the top of the page. No intellisense :(
- Can use `@code {}` blocks in `.razor` files OR code-behind with a `MyComponent.razor.cs` with a `public partial class MyComponent` in it.  
- Can't F12 on custom Razor components :(  
- VS has nice file nesting for all this, so the `.razor` is top level, with scss, css, and cs all hiding under it.  

{{< image path="img/VSFileNesting" alt="Code-behind, SASS, and CSS files all nested under Razor file" >}}


# Conclusions  
There are many little niggles plus the massive lack of hot-reload, but ultimately it was a successful trek into Blazor.  
It's quite different to my normal web dev, so many of my frustrations were centered on not knowing Razor syntax, or how to bind properly, etc.  

Does it offset not having to write Javascript?  
It might, time will tell.  