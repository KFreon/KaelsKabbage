---
title: "Cross platform options: A very shallow exploration"
date: 2023-09-04T14:25:01+10:00
type: "post"
slug: "cross-platform-options-a-very-shallow-exploration"
tags: ["maui", "flutter", "blazor", "svelte", "capacitor"]
---

You're starting a new app, which cross platform mobile framework should you choose?  
You've had Cordova apps, and they made you sad, you've heard "why not native?", and decided that's not viable (for your own reasons), so what to do?  
Well I built a similar toy app in a few of the big frameworks to compare them extremely subjectively (with some objectivity sprinkled in)  
Honestly, I still don't know which of them I like best.  

<!--more-->  

A cross platform app is common for many clients, as they want users to be able to do their thing on a laptop, iPhone, Galaxy, fridge, etc.  
As devs, we tend to want one codebase to rule them all, instead of one for web, one for iOS, and one for Android.  

Which inevitably leads us...here.  

{{< image path="img/Architect" alt="The matrix was written in Javascript all along!" >}}

The contenders I've decided to run with are:  
- Flutter, Google's shiny cross platform framework using Dart.  
- MAUI, Microsoft's shiny new cross plat- wait...that's just Xamarin Forms in a trenchcoat!  
- Capacitor, the new Cordova  

> I know there's more, but these are my choices.  

{{< toc levels=2 >}}


# Setup  
I'm using the tools current at the time of publishing, and I'll be creating a silly app that uses some basic controls like popups, inputs, database, etc.  

- NET 8 preview  
- Flutter 3.10.6
  - Dart SDK 3.0.6  
- Node 18.16.0  
- SQlite database (separate for each project)  
- Svelte 4.0.5

# Flutter
Dart is an interesting language, felt like SwiftUI (even though Dart was first)  
Flutter + Dart does things like Widgets, Futures, strong typing with `late` to escape, await/async, etc.  
It's composition-based such that components are wrapped rather than adding attributes to them.  

> You might also notice that the app quality gets progressively worse as I get bored ☹️

{{< video path="img/Demo_Flutter" alt="Flutter is interesting and looks pretty nice" >}}

## Things I like  
- Default styles are nice and modern  
- Presents good default icons too  
- Builds native apps for loads of different architecture targets  
- Hot reload is amazing in every environment  
- State management is decent out of the box  
  - Better state management achieved with `flutter pub add provider`, etc  
- Single line constructors like `MyType({required this.name, this.age})` where `name` is required, but `age` isn't.  
- Lists are cool
  - Spread operator is a thing  
  - Can have logic directly in the list initaliser, e.g. `if(CONDITION) {<Row ... />}` to conditionally have an element in a list  
- Good IDE refactor support  
- General syntax things are cool, Futures, state updates, map/reduce, etc

## Things I don't like  
- Everything feels inconsistent  
  - `bool vs String` casing  
  - `const vs final` where it's `const` in one context, but `final` in another  
- Constructors can be finicky  
  - Sometimes you can have `const` and do things in the constructor, but sometimes you can't, and I'm not sure when is which.  
- `Row` doesn't have a background, but `Container` does  
- Styles aren't discoverable. Should I use `TextStyle` or `ButtonStyle`? Obvious once you see it, but if you don't already know those styles exist, you'll never know.  
- Sometimes colors need an extra wrapper
  - Text --> `TextStyle(color: Colors.Green)`
  - ElevatedButton --> `ButtonStyle(backgroundColor: MaterialStatePropertyAll<Color>(Colors.Green))`
- `TextInput` is a thing, but the one you want is `TextField` which has the actual inputs
- Naming in general is frustrating
  - `FractionallySizedBox`, `Flex`, `SizedBox`, `Container`, what's the difference between all these?  
- There's `ListView` and `UnmodifiableListView`, but they're definitely not interchangable. 
  - In C#, there's underlying differences, but they're both lists at least.  
- Modals are called to open, but closed from inside the modal  

I'm sure all of this has a reason, and I should just learn, but it feels like an unusual obstacle.  
You learn a pattern, like `const`, but it doesn't FEEL consistent.  

Here's an example of the `build` method a widget:  

```dart
Widget build(BuildContext context) {
  final myState = context.watch<MyAppState>();

  return Center(
      child: Column(children: [
    const Icon(Icons.airplane_ticket, color: Colors.purple, size: 50),
    Padding(
      padding: const EdgeInsets.all(8.0),
      child: Row(mainAxisAlignment: MainAxisAlignment.center, children: [
        SizedBox(
          width: 100,
          child: TextField(
              onChanged: (value) => setState(() {
                    newCoolDude = value;
                  }),
              decoration: const InputDecoration(
                  border: UnderlineInputBorder(), labelText: "Your name")),
        ),
        Column(children: [
          const Text("Cool?"),
          Checkbox(
              value: isCoolDude,
              onChanged: (value) => setState(() {
                    isCoolDude = value ?? false;
                  }))
        ])
      ]),
    ),
    ElevatedButton(
        style: const ButtonStyle(
          backgroundColor: MaterialStatePropertyAll<Color>(Colors.green),
        ),
        onPressed: newCoolDude.isNotEmpty
            ? () {
                myState.addADude(CoolDude(
                    id: myState.getNextId(),
                    name: newCoolDude,
                    areTheyACoolDude: isCoolDude));
                setState(() {
                  newCoolDude = "";
                });
                Navigator.pop(outsideBuildContext);
              }
            : null,
        child: const Text("Add", style: TextStyle(color: Colors.white)))
  ]));
}
```  

Padding wrapping sizing wrapping columns...  
I find it very difficult to read and understand, but perhaps I just need to git gud.   

{{< image path="img/GitGud" alt="Git gud scrub" >}}

## Overall thoughts as a C# and web dev
I don't think I like it, but it feels like my own bias/thought patterns make me not like it, not that it's actually a bad experience.  
Composition especially just doesn't click for me.  
However, it seems easy enough to learn if required and well tooled, leading to a general recommendation.  


# MAUI  
Calling it Xamarin Forms is perhaps a bit harsh, it seems quite a bit of rework/fixing has gone into it, but it really is just Xamarin Forms++.  
I will say that while that's true, the experience feels better to me than XF did.  
I've written about [MAUI before]({{< ref "/posts/2023-06-10-my-journey-with-maui/index.md" >}}) and it was...fine.  It's fairly middle-of-the-road, decent but not amazing.  

{{< video path="img/Demo_MAUI" alt="Microsoft is pushing MAUI, and it turns out to be ok" >}}

## Things I like  
- C#! Yay!  
- Familiar Microsoft things
  - DI
  - Json handling
  - EFCore
  - etc
- XAML isn't terrible, Intellisense is decent  
- Decent community support with the [Community Toolkit](https://github.com/CommunityToolkit/Maui)  

## Things I don't like  
- Familiar Microsoft things  
  - Hot reload is...questionable  
  - Sometimes Visual Studio just doesn't work
  - Barely supported anywhere BUT VS  
- Familiar Xamarin Forms issues ☹️  
- New MAUI issues ☹️
  - Scrolling issues  
  - Performance  
  - `ListView` sometimes good, sometimes obscurely bad  
- EFCore has some compatibility fun
  - Design tools can't target MAUI project
  - MAUI project can't reference executable project
  - Executable project required for migrations
  - So three projects required. MAUI, DB, DB migrations target  
> See [the code](#github-links) for my solution to this. 

## Overall thoughts as a C# and web dev
It's easy to pick up and run with, but does follow the usual dotnet kind of thing where everything is achievable, but sometimes feels more verbose than necessary.  
A definite recommend, which is interesting considering Xamarin Forms would be an avoid.  

# MAUI Blazor Hybrid??
Blazor can be cross platform? Apparently yes!  
Feels pretty nice too, considering it's just Blazor with a MAUI shell.  
That shell is very thin and I didn't need to consider it with my silly app.  
> Perhaps using things like camera or biometrics would require additional work?  

I've written about [Blazor]({{< ref "/posts/2022-08-09-another-go-at-blazor/index.md" >}}) before as well, and I found it to be above average.  

{{< video path="img/Demo_MAUIBlazor" alt="Blazor can be wrapped in MAUI?? Yes! It can!" >}}

## Things I like  
- Hot reload is pretty good (remember to tick "Reload on file save"!)  
- All the good Blazor things  
- Familiar to web devs AND C# devs  

> `Ctrl + shift + i` shows the debug tools. It was pain until I found this.  

## Things I don't like  
- All the existing silly Blazor things  
- Windows can fail to deploy (usually on the very first deploy ever)   
  - Fails to find the splash screen, even though it's there
  - Rebuild x2 and it seems to work  


# Cordova, the fallen hero  
I didn't even try Cordova for this, as I've had a growing number of issues in all my Cordova projects, as have my colleagues.  
Flakey builds, unmaintained but critical plugins (push plugin, looking at you), confusing chains of plugins, confusing and inconsistent configuration.  
I still have several Cordova projects, and I'm annoyed every time I have to touch it.  
It was good while it lasted 😭  

# Svelte + Capacitor, the new Cordova  
[Capacitor](https://capacitorjs.com/) allows us to use familiar web frameworks and tooling and only have to worry about the cross platform part when we want to.  
I decided to try out [Svelte](https://svelte.dev/) which has been on my radar for a while.  

{{< video path="img/Demo_SvelteCapacitor" alt="Svelte and Capacitor make a nice combo" >}}

## Svelte  
Svelte comes in two pieces, Svelte and SvelteKit, both of which are used here.  
I'd describe it as HTML++, as it augments the existing HTML markup with some useful additions, like loops, variables, etc.  
Its syntax and behaviours are familiar to a web dev and it's additions to the HTML markup are clear and fairly simple to understand (mostly).  
SvelteKit is the building side of the fence and has many adapters enabling various different approaches (nodejs, static pages, etc)   

## Capacitor  
Capacitor was reasonably simple to setup.  
The instructions on the website are all I needed to get going.  
SvelteKit has an [adapter](https://kit.svelte.dev/docs/adapter-static) which can render to static pages, as long as there's no server-side stuff, so I moved that to a quick backend in C#.  
From there, it's `cap add android`, `cap sync`, `cap open android`.  
The only catch is adding the required plugins, which I found was fine here, but was a more significant piece of work when migrating existing projects.  

## Things I like  
- SvelteKit uses directory routing, where each directory has a page in it that responds to the route.  
- Vite hot reload is amazing as usual  
- It feels like HTML++, and so is very accessible to existing web developers  
- Capacitor is easier than Cordova  
- Native projects!  

## Things I don't like  
- Things it adds to HTML are good, but sometimes undiscoverable  
  - e.g. `bind:...` are a bit confusing
- Some magic like server-side `data` variable  

> I had trouble getting Typescript working, likely just me though


## Overall thoughts as a C# and web dev  
This is an excellent option, as you get it all with some limitations.  
My silly toy project didn't run into any issues, but I can imagine scenarios where using device features becomes a bit painful, but for that, you have full access to the native projects as required so perhaps not even an issue.  
The other really nice thing about this approach is the flexibility to use almost any web framework/libraries you want.  
It's especially great when you're just wrapping a website for the appstores. 

# Recommendation for a new project  
If you're a C# dev, MAUI or Blazor/MAUI is a decent choice.  
If you're a web dev OR wrapping an existing website, `Web framework of choice` + Capacitor is a good choice.  
If you're a fan of new shiny things, Flutter might be the one for you.  

# Other conclusions and thoughts  
This is hardly an exhaustive list, or even a deep investigation into the options presented.  
However, I think there's some learnings here despite the toy examples.  
The time implementing each was about even, although it's hard to measure considering I was doing significant learning every step of the way.  
My original intent was to compare the picking up experiences of each approach, but I'm not sure I got a good measure of that.  
My gut says they're all on a pretty even level for onboarding.  

There's much more for me to learn on the mobile journey, however I know that I'm working with Capacitor and MAUI right now.  
It's likely most of my powers will go into those two for the time being.  



# Github links  
It's all awful code.  
I'm sorry, I started lazy and only got lazier.  

[Flutter](https://github.com/KFreon/flutter_demo)  
[MAUI](https://github.com/KFreon/MAUI_CoolDudes)  
[MAUI + Blazor](https://github.com/KFreon/MAUIBlazorHybrid_CoolDudes)  
[Svelte](https://github.com/KFreon/SvelteCoolDudes)  