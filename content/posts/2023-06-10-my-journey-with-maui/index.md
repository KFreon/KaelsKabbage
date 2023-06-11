---
title: "What does a production MAUI app look like?"
date: 2023-06-10T12:39:04+10:00
type: "post"
slug: "production-maui-app"
tags: ["maui"]
---

In the past, I've played with MAUI in toy projects but recently I decided to try and upgrade a Xamarin Forms app to MAUI.  
That went poorly with the [Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/maui/migration/upgrade-assistant) so I started with a fresh project and migrated across.
Here's a few tips and tricks and common patterns for a MAUI app that I found useful.  

<!--more-->  

I'm not going to claim these are the only methods of getting good outcomes, or that I've even measured the outcomes...  
But I've applied some common design patterns to this app and gotten a result I like.  

Let's have a closer look.

{{< toc levels="two" >}}

# Goals  
At the end of this, I want a project that has:
- Familiar patterns for C# and web devs  
- Custom splash screen
- Custom app icon
- Popups
- Data persistence  

# Result
{{< video path="img/MAUIResult" alt="The final result. Looks rubbish, but style isn't my strong suit." >}}

# Setup  
- NET 7
- Android API 33  
- Visual Studio 2022  
- Starting with standard VS template for MAUI app  
  - Removed all other platforms, including from the csproj  

With all that out of the way, let's start with the guts of any production application: Dependency injection.

# Dependency Injection  
Dependency injection? Isn't that a bit overkill for a mobile app?  
I'd argue "no" considering it's not all that heavy and it's a familiar pattern for dotnet developers, which is one of the main goals.  
Another major reason I want to use it here is that it makes working with EFCore easier.  

The setup and usage is essentially the same as in aspnetcore:

{{< splitter >}}
{{% split side=left title="MauiProgram.cs" %}}
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
{{% /split %}}
{{% split side=right title="ServiceCollectionExtensions.cs" %}}
```cs
public static IServiceCollection RegisterViewModels(this IServiceCollection services)
{
  services.AddTransient<MainPageViewModel>();
  services.AddTransient<ContentPopupViewModel>();
  return services;
}
```
{{% /split %}}
{{< /splitter >}}  

Note that I have to register my views and view models so they can take advantage of DI.  
I've created extension methods to register them to keep `MauiProgram.cs` clean, and logically group things.  

Consuming DI is done via constructor injection or `IServiceProvider` as normal, however there is a [current bug](https://github.com/dotnet/maui/issues/11485) where the very first view can't have constructor injection for it's view model.  
As such, the first view (usually `App.cs`) has to resolve it with `IServiceProvider`, but the rest can be as normal.  
{{< splitter >}}
{{% split side=left title="App.cs" %}}
```cs
public App(IServiceProvider services, IConfigurationService config)
{
	InitializeComponent();
	MainPage = services.GetRequiredService<MyMainPage>();  // Can't inject the view model, have to resolve it here.
	this.config = config;
}
```
{{% /split %}}
{{% split side=right title="MainPage.cs" %}}
```cs
public MainPage(MainPageViewModel viewModel)
{
  InitializeComponent();
  BindingContext = viewModel;
}
```
{{% /split %}}
{{< /splitter >}}  

One thing I ran into almost immediately with this setup was that view models tended to have async setup, fetching data from databases or networks and such.  
So how can we handle those situations?  

## View model configuration  
There's a couple of ways I want to configure my view models.  
The main one is the one I alluded to above: Long running initialisation setup.  

### Async setup  
Views have a lifecycle event for when it appears to the user: `OnAppearing` which can be async.  
We can use that to trigger any async setup for the view model and/or any other setup for the view we need that can't be done as part of the constructor.  

```cs
protected override async void OnAppearing()
{
  await (BindingContext as MainPageViewModel)?.Initialise();
  base.OnAppearing();
}
```

There's also an `OnDisappearing` lifecycle event for cleanup if required, but I haven't needed it yet.  

Another scenario I quickly discovered was having to pass information from one view to another.  

### Passing info into view models  
The only time I needed to do this was when using popups and I wanted to pass text or info into it.  
I liked the way that aspnetcore and other libraries handle their configuration.  
Nick Chapsas talks about this in one of his [videos](https://www.youtube.com/watch?v=kIkbGXLkc-g), and it looks like:  
```cs
private void ShowInfoPopup()
{
  popupService.ShowPopup<ContentPopup, ContentPopupViewModel>(model =>
  {
    // Configure the model here
    model.Title = "Info about this app";
    model.Content = "This is an attempt to show how MAUI can be used more easily in a production scenario. That is, I'm using processes like those described in this app in a production app.";
  });
}
```

It's a bit of an involved process to get that for the view models:  
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

# Popups  
Popups are a common requirement in mobile apps.  
I'm using the popups from the [Maui Community Toolkit](https://github.com/CommunityToolkit/Maui), but I don't think there's a template for it so my method is:  
- Add a `ContentPage` template  
- Add `xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit` to the top of the xaml
- Change `<ContentPage` to `<toolkit:Popup>`
- Change the code behind from `: ContentPage` to `: Popup`  

> After I did this, I saw that there's a `ModalStack` on the `Navigation` property of views. I'm not sure if I could have used that instead...

## Common settings  
- `CanBeDismissedByTappingOutsideOfPopup="False"`
  - Prevents closing when tapping outside
- `Color="Transparent"` to hide the popup background
  - This gives more control over the UI  

## Using the Popup
I've already described most of it above, but here's a refresher on how I'm opening a popup.  
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

Closing a popup is done by calling `.Close()` on the popup itself, but we want to be able to close the popup from any view model:  
`popupService.HidePopup() // close the current top popup` 

My `PopupService` has a stack: `private Stack<Popup> popups = new();` and the above function `HidePopup` looks like this:
```cs
public void HidePopup()
{
  popups.Pop().Close();
}
```

# EFCore for persistence  
Most apps want to save data somewhere, and we could do that on the server, fetching and sending data as required, but there's usually a requirement to store things locally for some purpose.  
I'm using EFCore because I'm familiar with it, as are many of my colleagues are as well, and thus fits the "familiar feeling" requirement.  

Let's use Sqlite here as it's "lite" and the one used in the Microsoft samples 😀
```cs
// MauiProgram.cs
var migrationAssembly = typeof(Repo).Assembly.GetName().Name;
builder.Services.AddDbContext<IRepo, Repo>(opts =>
{
	var dbPath = Path.Combine(FileSystem.AppDataDirectory, "database.db3");
	opts.UseSqlite($"Filename={dbPath}", x=> x.MigrationsAssembly(migrationsAssembly));
});
```  

But wait, what's that `.MigrationsAssembly` thing?  

## "External" Migrations  
The [dotnet ef migrations CLI](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli) doesn't support the MAUI targets, so we need to use a separate project.  
The [docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli) have some details, but I found it a bit all over the place, so here's a flow chart showing the resulting projects and links.  

```goat
                .
        .------> DB <------.
        |                  |
DB Entry Project      MAUI Project
```

The process is:
- Create a new class library and put all DB contexts and models in there
- Create a console app that references the DB project and adjust `program.cs` to reference the `IDesignTimeDbContextFactory<Repo>`
  - According to the docs and StackOverflow, this all works because the `dotnet ef cli` tools need an entrypoint, and they don't support the MAUI project targets yet.  
```cs
Console.WriteLine("This is the efcore CLI migrations entrypoint. Use 'dotnet ef migrations add <migration name> --project MyProject/MyProject.csproj --startup-project DB/DB.csproj' to add a new migration");

public class DesignRepo : IDesignTimeDbContextFactory<Repo>
{
  public Repo CreateDbContext(string[] args)
  {
    return new Repo(new DbContextOptionsBuilder<Repo>().UseSqlite("doesnt matter").Options);
  }
}
```
  
- Generate migrations with `dotnet ef migrations add <migration name> --project MAUI/MAUI.csproj --startup-project DBEntry/DBEntry.csproj`

> I feel like I could put the DB classes in the same project as the entry project, but I haven't tested that.  

EFCore needs a connection string, which we usually get from `appsettings.json`.  
We can do that!  

# Configuration through `appsettings.json`  
While this is similar to aspnetcore, there's a bit of extra setup here and requires some nuget packages:  
- Microsoft.Extensions.Configuration.Binder  
- Microsoft.Extensions.Configuration.Json  

```cs
// I've set the appsettings.json as an EmbeddedResource
// So we need to get it's path with assembly
var ass = Assembly.GetExecutingAssembly();
var debugSettings = ass.GetManifestResourceStream("MAUIExampleWithEFCore.appsettings.json");

// Add appsettings.json file
var configRoot = new ConfigurationBuilder()
    .AddJsonStream(debugSettings)
    .Build();

// Bind to Config class
var config = new Config();
configRoot.Bind(config);

services.AddSingleton<Config>(config);
```

# Custom views/controls  
It wasn't long before I wanted to encapsulate some view components for reuse.  
This is especially true for xaml compared to html as it feels more verbose.  
For some reason, this took me a bit of time to figure out, but it's actually pretty simple.  

## Nicer Text Input
Here's a nicer text input control with some nicer placeholder behaviour. 

{{< splitter >}}
{{< split side=left title="Before" >}}
{{< video path="img/NormalEntry" alt="Default Entry behaviour" width="25rem" >}}
{{< /split >}}
{{< split side=right title="After" >}}
{{< video path="img/NiceEntry" alt="Nicer entry behaviour, similar to Material design" width="25rem" >}}
{{< /split >}}
{{< /splitter >}}  

Used thusly:
```xml
<!-- With this namespace added -->
<!-- xmlns:controls="clr-namespace:MAUIExampleWithEFCore.Views.Controls" -->

<controls:NiceEntry Text="{Binding Name}" Placeholder="Name?" WidthRequest="100" LabelColor="White" TextColor="White"/>
```

It's quite a bit of backend code to make it work though, primarily in all the bindable properties I want to expose.  
{{< splitter >}}
{{% split side=left title="XAML" %}}
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            x:Class="MAUIExampleWithEFCore.Views.Controls.NiceEntry"
            x:Name="niceEntry">
  <Grid BackgroundColor="Transparent" HeightRequest="50">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
      <RowDefinition Height="50"/>
    </Grid.RowDefinitions>

    <Entry Grid.Row="0" Grid.Column="0"
      Margin="0,0,0,0"
      Text="{Binding Source={x:Reference niceEntry}, Path=Text}" 
      ReturnType="{Binding Source={x:Reference niceEntry}, Path=ReturnType}"
      FontSize="{Binding Source={x:Reference niceEntry}, Path=FontSize}"
      BackgroundColor="{Binding Source={x:Reference niceEntry}, Path=BackgroundColor}"
      TextColor="{Binding Source={x:Reference niceEntry}, Path=TextColor}"
      Focused="Entry_FocusChanged"
      Unfocused="Entry_FocusChanged"
      Keyboard="{Binding Source={x:Reference niceEntry}, Path=Keyboard}"
      ReturnCommand="{Binding Source={x:Reference niceEntry}, Path=ReturnCommand}"
      IsPassword="{Binding Source={x:Reference niceEntry}, Path=IsPassword}"
    />

    <Label Grid.Row="0" Grid.Column="0"
      BackgroundColor="Transparent"
      Margin="0,10,0,0"
      AnchorX="0"
      AnchorY="0"
      TextColor="{Binding Source={x:Reference niceEntry}, Path=LabelColor}"
      FontSize="{Binding Source={x:Reference niceEntry}, Path=FontSize}"
      x:Name="thelabel"
      Text="{Binding Source={x:Reference niceEntry}, Path=Placeholder}"
      />
  </Grid>
</ContentView>
```
{{% /split %}}
{{% split side=right title="Code behind" %}}
```cs
public partial class NiceEntry : ContentView
{
    public NiceEntry()
    {
        InitializeComponent();
    }

    public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(NiceEntry), false);

    public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(NiceEntry), null);

    public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(NiceEntry), null, BindingMode.TwoWay);

    public static BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(NiceEntry), ReturnType.Default);

    public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(NiceEntry), null);
    public static BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(NiceEntry), null);

    public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(NiceEntry), Keyboard.Default);

    public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(string), typeof(NiceEntry), "24");

    public static BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(Command), typeof(NiceEntry), null);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color LabelColor
    {
        get => (Color)GetValue(LabelColorProperty);
        set => SetValue(LabelColorProperty, value);
    }

    public string FontSize
    {
        get => (string)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            var noText = string.IsNullOrEmpty(value);

            if (!noText)
            {
                MakeLabelSmall();
            }

            if (noText)
            {
                RestoreLabel();
            }
        }
    }

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set => SetValue(ReturnTypeProperty, value);
    }

    public Command ReturnCommand
    {
        get => (Command)GetValue(ReturnCommandProperty);
        set => SetValue(ReturnCommandProperty, value);
    }

    private void MakeLabelSmall()
    {
        thelabel.ScaleTo(0.7, easing: Easing.CubicInOut);
        thelabel.TranslateTo(0, -20, easing: Easing.CubicInOut);
    }
    private void RestoreLabel()
    {
        thelabel.ScaleTo(1, easing: Easing.CubicInOut);
        thelabel.TranslateTo(0, 0, easing: Easing.CubicInOut);
    }

    private void Entry_FocusChanged(object sender, FocusEventArgs e)
    {
        if (e.IsFocused)
        {
            MakeLabelSmall();
        }

        if (string.IsNullOrEmpty(Text))
        {
            RestoreLabel();
        }
    }
}
```
{{% /split %}}
{{< /splitter >}}  

`BindableProperty` creates a property that is exposed to the xaml view and configures it's behaviour (default values, binding directions, etc)  
This allows other bindings/values to be passed in from the parent.  

An important difference between these custom controls and normal views is the binding context.  
Normal views bind to themselves (or the viewmodels, usually set somewhere), but for custom controls, the binding context is actually wherever it's used.  
As such, when we bind to our properties so we can display them, we need to tell the xaml that they're in the control, not in the parent.  
We do that by setting `{Binding Source={x:Reference controlName}, Path=Something}` for the bindings.  
> Can we set the binding context in the constructor? Like `BindingContext = this;`?   
> Not sure, but this way works nicely.

# Styles and resource dictionaries
Stying isn't done with CSS (good/bad?) but we do get some nice intellisense through Visual Studio.  
There are a bunch of ways to handle this, but the ways I generally found worked nicely were:
- Encapsulated styles in custom components (see the NiceCheckbox above)
- Resource dictionaries at any level
  - Allows setting default styles for sections of the app without manually specifying each one
- Directly on elements
  - Just like html + css, you shouldn't do this

An example style is below and sets some various properties: 
```xml
<Style TargetType="Frame" x:Key="myFrame">
  <Setter Property="HasShadow" Value="False" />
  <Setter Property="BorderColor" Value="{AppThemeBinding Light={StaticResource Gray200}, Dark={StaticResource Gray950}}" />  <!--> This is some custom default stuff, not sure how it works <-->
  <Setter Property="CornerRadius" Value="8" />
</Style>
```

It has the `x:Key` attribute which means it'll only be applied if directly referenced like:
```xml
<Frame Style={StaticResource myFrame} ... />
```

If `x:Key` is omitted, the style applies to all matching types "below" where the style is referenced in the tree.
i.e. If it's in the `App.xaml` resource dictionary, it's essentially a global style, but if it's defined in a `StackLayout.Resources` block, it only applies to the resources within that StackLayout.  

# Some smaller tidbits
## Animations
Animations seem to be only defined in code behind, no xaml.  
```cs
someElement.ScaleTo(2);  // Translate, rotate, etc
```

## Custom fonts
Add fonts to Resources folder, then adjust `MauiProgram.cs`:
```cs
var builder = MauiApp.CreateBuilder();
builder
  .UseMauiApp<App>()
  .UseMauiCommunityToolkit()
  .ConfigureFonts(fonts =>
  {
    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

    // Add more fonts to the Fonts folder, set as MAUI font, and add here as above
    fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
  });
  //...
```   

## Frame vs Border  
Xamarin Forms had a `Frame` component which allowed adding a border, corner radius, shadow, etc to views.  
`Frame` is deprecated in MAUI, and it's supposed to still work, but I get exceptions like "No parameterless constructor for FrameRenderer".  
Instead, we can use `Border`: 
```xml
<Border 
  BackgroundColor="{StaticResource ThemeWhite}"
  WidthRequest="560"
  VerticalOptions="Center"
  HorizontalOptions="Center"
  Padding="0">
  <Border.StrokeShape>
    <RoundRectangle CornerRadius="8"/>
  </Border.StrokeShape>
  <Grid...
```  

## Even smaller bits and recommendations  
- Ensure you use `x:DataType="YourViewModel"` to get design time intellisense
  - Usually requires setting `xmlns:viewmodels="path-to-viewmodels" in the view
- Timers are different from Xamarin Forms, but similar to normal C# timers:
```cs
var timer = Dispatcher.GetForCurrentThread().CreateTimer();
timer.Interval = TimeSpan.FromSeconds(2);
timer.Tick += async (_,_) => await DoSomething();
timer.Start();
```
- Updating UI elements usually has to be done from UI thread, which can be marshalled like: 
```cs
MainThread.InvokeOnMainThreadAsync(async() => await DoSomething());
```
- Default colours like splashscreen background, status bar, and other accent colours are defined in `Platforms/.../colors.xml`
  - `ColorPrimaryDark` = Status bar colour
  - `ColorAccent` = thins like Entry underlines, etc  
- Custom splashscreen can be set by copying image into `Resources/Splash` and setting as `MauiSplashScreen`
  - Further config may be required in the csproj  
- Custom icon can be set by copying image into `Resources/AppIcon` and setting as `MauiIcon`
  - Further config may be required in csproj
  - Also requires setting the correct name in `Platforms/Android/AndroidManifest.xml`:
  ```xml
  <application android:icon="@mipmap/your-custom-icon-name" ...
  ```
- Intellisense sometimes breaks, usually on custom controls or Popups
  - Renaming it to a `ContentPage` and back usually fixes it
- Changes to code don't auto deploy (sometimes they do but not often)
- `BindableProperty` has to be a string? 
  - I'm not exactly sure about this, but when creating a component that rebound `FontSize` I had to make it a string instead of a numeric type. 
- Scrolling
  - I had difficulties getting scrolling working, perhaps because it was in a Popup.
  - `CollectionView` with an `ItemTemplate` set tended to work better than just a `ScrollView`

You can see the full solution [on Github](https://github.com/KFreon/MAUIExampleWithEFCore) for more context and information.
