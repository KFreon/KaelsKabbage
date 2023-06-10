---
title: "My Journey with MAUI"
date: 2023-06-10T12:39:04+10:00
draft: true
type: "post"
slug: "my-journey-with-maui"
tags: ["maui"]
---

I've played with MAUI in the [past]({{< ref "/posts/29-swiftui-maui/index.md" >}}), but it was always as toy projects.  
Recently, I decided to try and upgrade a Xamarin Forms app to MAUI, which went poorly, so I've started with a fresh project and I've learned some things about MAUI and how to architect a MAUI project for success!  


<!--more-->  

# Goals  
At the end of this, I want a project that has:
- Familiar patterns for C# and web devs  
- Custom splash screen
- Custom app icon
- Popups
- Data persistence  

# Setup  
- NET 7
- Android API 33  
- Visual Studio 2022  
- Starting with standard VS template for MAUI app  
  - Removed all other platforms, including from the csproj  

# Dependency Injection  
Dependency injection has many advantages, but the main one for me in this project is that it's a familiar pattern for dotnet developers.  
Eventually, it'll allow simpler testing as well, but I haven't gotten that far yet.  
Another major reason I want to use it here is that it makes working with EFCore easier (more on EF later).  

We can register our services as we do in Aspnetcore:
```cs
public static MauiApp CreateMauiApp()
	{
    var builder = MauiApp.CreateBuilder();
		builder
    .UseMauiApp<App>()
    //...

    builder.Services.AddTransient<MyService>();
    //...

		return builder.Build();
	}
```

However, if we want to be able to take advantage of DI in our views or view models, we need to register them as well.  
I've created extension methods to register them to keep `MauiProgram.cs` clean, and logically group things.  
e.g.  
```cs
public static IServiceCollection RegisterViewModels(this IServiceCollection services)
{
    services.AddTransient<MainPageViewModel>();
    services.AddTransient<ContentPopupViewModel>();
    return services;
}
```

Consuming DI is done via constructor injection or `IServiceProvider` as normal, however there is a [current bug](https://github.com/dotnet/maui/issues/11485) where the very first view can't have constructor injection for it's view model.  
As such, the first view (`App.xaml` for me and most I'm sure) looks like this:
```cs
public App(IServiceProvider services, IConfigurationService config)
{
	InitializeComponent();
	MainPage = services.GetRequiredService<MyMainPage>();  // Can't inject the view model, have to resolve it here.
	this.config = config;
}
```

All other views should be able to be done normally:
```cs
public MainPage(MainPageViewModel viewModel)
{
  InitializeComponent();
  BindingContext = viewModel;
}
```

## View model configuration  
One thing I ran into almost immediately with this setup was that view models tended to have async setup, fetching data from databases or networks and such.  

### Async setup  
Views have a lifecycle hook for when it appears to the user: `OnAppearing` which can be made async.  
We can use that to trigger any async setup for the view model and/or any other setup for the view we need that can't be done as part of the constructor.  

```cs
protected override async void OnAppearing()
{
  await (BindingContext as MainPageViewModel)?.Initialise();
  base.OnAppearing();
}
```

### Passing info into view models  
Another scenario I quickly discovered was having to pass information from one view to another.  
This was only for popups in my case, so the below code is specific for that but the basic idea is:
- Build a setup action
- Resolve the view and view model
- Run setup action on the view model
- Set the view (as the main page, or in my case, show the popup)

{{< splitter >}}
{{% split side=left title="Sync Version" %}}
```cs
public void ShowPopup<T, V>(Action<V> viewModelSetup = null) where T : Popup where V : ViewModelBase
{
    var mainPage = App.Current?.MainPage ?? throw new MissingMethodException("Main page is null");
    var popup = services.GetRequiredService<T>();
    var viewModel = services.GetRequiredService<V>();
    viewModelSetup?.Invoke(viewModel);
    popup.BindingContext = viewModel;
    mainPage.ShowPopup(popup);
    popups.Push(popup);
}
```
{{% /split %}}
{{% split side=right title="Async Version" %}}
```cs
public async Task ShowPopup<T, V>(Func<V, Task> viewModelSetup = null) where T : Popup where V : ViewModelBase
{
    var mainPage = App.Current?.MainPage ?? throw new MissingMethodException("Main page is null");
    var popup = services.GetRequiredService<T>();
    var viewModel = services.GetRequiredService<V>();
    await viewModelSetup?.Invoke(viewModel);
    popup.BindingContext = viewModel;
    mainPage.ShowPopup(popup);
    popups.Push(popup);
}
```
{{% /split %}}
{{< /splitter >}}  

The setup method style is easy to understand and similar to how many of the new aspnetcore configurations are done.  
```cs
private void ShowInfoPopup()
{
    popupService.ShowPopup<ContentPopup, ContentPopupViewModel>(model =>
    {
        model.Title = "Info about this app";
        model.Content = "This is an attempt to show how MAUI can be used more easily in a production scenario. That is, I'm using processes like those described in this app in a production app.";
    });
}
```

# Popups  
As alluded to above, I'm using the popups from the [Maui Community Toolkit](https://github.com/CommunityToolkit/Maui).  
I couldn't find a Visual Studio template for it, so my method is:
- Add a `ContentPage` template  
- Add `xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit` to the top of the xaml
- Change `<ContentPage` to `<toolkit:Popup>`
- Change the code behind from `: ContentPage` to `: Popup`  

## Common settings  
- `CanBeDismissedByTappingOutsideOfPopup="False"`
  - Prevents closing when tapping outside
- `Color="Transparent"` to hide the popup background
  - This gives more control over the UI  

## Using the Popup
I want it to be simple to use and I've already alluded to the process above:  
```cs
// No view model configuration
popupService.ShowPopup<MyPopup>();

// With view model config
popupService.ShoPopup<MyPopup, MyPopupViewModel>(model => {
	model.MyValue = "some value";
	model.SubmitAction = new Command(() => 
	{
		popupService.HidePopup();  // Hides this popup because it's the newest
		DoSomeSubmitAction();
	});
});
```

Closing is done by calling `.Close()` on the popup itself, which means we need to keep track of it.  
The `PopupService` has a stack: `private Stack<Popup> popups = new();` and the above function `HidePopup` looks like this:
```cs
public void HidePopup()
{
  popups.Pop().Close();
}
```

