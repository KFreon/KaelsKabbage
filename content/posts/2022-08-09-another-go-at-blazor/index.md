---
title: "Another go at Blazor: Complex State?"
date: 2022-08-09T15:58:43+10:00
type: "post"
slug: "another-go-at-blazor"
tags: ["blazor"]
---

Previously on [Blazor: Good and Bad](/25-blazor-net5) I played with Blazor and made a toy app to test things out.  
I came to the conclusion that some was good, some was bad, time will tell.  
Now that I've come back to it with a much more complex app, my opinion hasn't really changed, but I did have a go at more complex state management.    

<!--more-->  

My coverage and opinions from last time are still valid, although some improvements have been made:
- Hot reload worked better  
- F12 works on custom components!  
- Razor/cshtml intellisense is more stable

My conclusion from last time is also still valid: *Not bad, but poor discoverability, weak and sometimes unstable Intellisense, but generally ok.*  

Something I didn't have last time was have complex state or behaviours.  

# Blazor Server State Management  
There's some [official documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-6.0&pivots=server) around this, which is good, because it's a bit difficult to wrap your head around the lifetime of objects in Blazor Server.  

e.g. 

```html
@page "/"

<div>
  <CascadingValue Value="AThing">
    <SomeComponent>
  </CascadingValue>
</div>

@code {
  public SomeClass AThing {get;set;}

  protected override async Task OnInitializedAsync() 
  {
    AThing = await CreateThing();
  }
}
```

**When does `AThing` get created? How many times? When you visit the page?**  

It turns out it's technically created [twice](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-6.0#component-initialization-oninitializedasync) due to how the whole thing works.  
More interestingly, it's NOT recreated on navigations as long as it's still within scope, like if it's in the root/host component.  
If you refresh, open a new tab, etc it's created, but persisted through navigations.

## How I'm managing state
I decided to go with a singleton state (ick?) but I think it's ok in this case.   

- Created at startup and registered as singleton by the DI, then injected elsewhere, allowing replacement in tests if I ever write any  
- The reads are locked; The writes are handled by Cosmos and Blob Storage libraries which either replace or throw  
- Low number of users, collisions unlikely, consequences minimal  

This state has an `event` that all circuits subscribe to and trigger change detection for all connected circuits.  
It keeps things simple and works pretty well, but I don't know if it's a scalable solution.  

Initially, I had a state per user like a normal system, but I wanted to reflect changes from other users in the UI.
An event passing the changes would have been sufficient here as well, but there would have been lots of reading and writing from Cosmos.  
That feels like a bit of an excuse to be lazy and have global state now that I've written it...
The idea is to just have a data caching/translation layer where all the UI talks to, and limit contact with Cosmos.  

Anyway, it works well enough here, and while I'm not convinced it's the best idea, I don't think it's a bad one either.  

{{< splitter >}}
{{% split side=left title="AppState" %}}
```cs
public class AppState
{
  private static SemaphoreSlim locker = new(1);
  public event Func<Task>? OnChange;
  public void NotifyStateChanged() => OnChange?.Invoke();

  // "Cached" items
  private List<Result> results = new();
  private List<UIDisplay> items = new();
  public IReadOnlyList<Result> Results => results;
  public IReadOnlyList<UIDisplay> Items => items;

  public async Task Refresh()
  {
      await locker.WaitAsync();
      var (results, items) = await LongInitialisationFromCosmos();
      locker.Release();
      NotifyStateChanged();
  }
}
```
{{% /split %}}
{{% split side=right title="Example View" %}}
```html
@page "/";
@Inject AppState State;

<div>
  <CascadingValue Value="State">
    <SomeComponent>
  </CascadingValue>
</div>

@code {
  protected override async Task OnInitializedAsync() 
  {
    State.OnChange += HandleStateChanged;
  }

  // Ensure we're on the UI thread for this circuit
  private Task HandleStateChanged() => InvokeAsync(() => StateHasChanged());
}
```
{{% /split %}}
{{< /splitter >}}  


# Edit 20-04-2023  
Some missing pieces to the above, and some new things I'm using:  
- To create the `AppState`, call the `Refresh` method from the `Index.razor` or `_host.cshtml` or somewhere high level via the `OnInitializingAsync`.  
- `NavLink` to easily create an `a` tag to Razor pages
  - `<NavLink href="my-@page-path">Go to the page</NavLink>`   