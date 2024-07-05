---
title: "Android: Foreground Services"
date: 2023-02-08T16:15:37+10:00
type: "post"
slug: "android-foreground-services"
tags: ["xamarin", "android"]
---

I have a Xamarin Forms app that needs to send location info (for example a shopping center assistant app) to the server every 5 seconds, but I know that the user will likely be using some other app during this time, so my app will be in the background.
How can I make that happen?

<!--more-->  

I might think to just implement it in the app as a timer that starts when the App does.  

```cs
// In the Android project of Xamarin Forms
class MainActivity {
  // etc

  App.Device.StartTimer(TimeSpan.FromSeconds(5), () => {
    SendVeryImportantUpdateInfo();
  });
}
```

I soon found that this was fine, until you switch to another app, and the timer stops.  
Turns out there are [limits to background activity](https://learn.microsoft.com/en-us/xamarin/android/app-fundamentals/services/#background-execution-limits-in-android-80) since Android 8, and it seems in that documentation that the app CAN do some background things, but not necessarily...
We can't rely on the grace of the Android OS to let the app do background things, so what can we do to ensure it still works?

# Foreground Services

[Foreground services](https://learn.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services) are those persistent notifications that show up in the notification area, indicating a long running activity. 
The most obvious for me is a music player, which displays the song details and controls regardless of what app I'm using. 
The Xamarin Forms docs are ok for this, but I came across a [Stackoverflow post](https://stackoverflow.com/questions/61079610/how-to-create-a-xamarin-foreground-service) that goes over it quite well, and mine is strongly based on it.  

{{% splitter %}}
{{% split side=left title="The Service" %}}
```cs
// In the Android project of Xamarin Forms

// Inheriting and attribute required. 
// Interesting double up...
[Service]
public class MyForegroundService : Service 
{
  public override IBinder OnBind(Intent intent)
  {
      return null;
  }

  public int ServiceId { get; }
  public const string START_ACTION = "START";
  public const string STOP_ACTION = "START";
  private bool isTimerRunning = true;

  public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
  {
    if (intent.Action == START_ACTION)
    {
      StartTheService(intent);
    }
    else if (intent.Action == STOP_ACTION)
    {
      StopTheService();
    }

    return StartCommandResult.Sticky;
  }

  void StopTheService()
  {
    isTimerRunning = false;
    StopForeground(true);
    StopSelf();
  }

  void StartTheService(Intent intent)
  {
    intent.AddFlags(ActivityFlags.SingleTop);
    intent.PutExtra("Title", "Message");

    Context context = global::Android.App.Application.Context;

    var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

    var notifBuilder = new NotificationCompat.Builder(context, ServiceId.ToString())
      .SetContentTitle("A Good Title")
      .SetSmallIcon(Resource.Drawable.clock_black)
      .SetContentIntent(pendingIntent)
      .AddAction(StopButton())
      .SetOngoing(true);

    // Building channel if API verion is 26 or above
    if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
    {
      NotificationChannel notificationChannel = new NotificationChannel(ServiceId.ToString(), "Title", NotificationImportance.High);
      notificationChannel.Importance = NotificationImportance.High;
      notificationChannel.SetShowBadge(true);

      var notifManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
      if (notifManager != null)
      {
        notifBuilder.SetChannelId(ServiceId.ToString());
        notifManager.CreateNotificationChannel(notificationChannel);
      }
    }

    StartTimerForLongRunningAction();

    StartForeground(ServiceId, notifBuilder.Build());
  }

  // Adds a button to the notification that stops the app and removes the notification
  private NotificationCompat.Action StopButton()
  {
      Context context = global::Android.App.Application.Context;
      var stopServiceIntent = new Intent(context, GetType());
      stopServiceIntent.SetAction(STOP_ACTION);
      var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);

      var builder = new NotificationCompat.Action.Builder(Android.Resource.Drawable.IcMediaPause,
                                                    GetText(Resource.String.mr_controller_stop),
                                                    stopServicePendingIntent);
      return builder.Build();
  }

  public void StartTimerForLongRunningAction()
  {
    App.Device.StartTimer(TimeSpan.FromSeconds(5), () =>
    {
      Console.WriteLine("Ding! Doing a timer thing.");
      return isTimerRunning;
    });
  }

  public override void OnDestroy()
  {
    StopTheService();
    base.OnDestroy();
  }

  public override bool StopService(Intent name)
  {
    StopTheService();
    return base.StopService(name);
  }
}
```
{{% /split %}}
{{% split side=right title="MainActivity" %}}
```cs
class MainActivity 
{
  protected override void OnCreate(Bundle savedInstanceState)
  {
    // Stop any running service from previous versions of this app
    // I'm not 100% sure what the behaviour is if this doesn't happen
    // Do we double up?
    // Does the old service get replaced?
    var service = new Intent(this, typeof(MyForegroundService));
    service.SetAction("STOP");
    StopService();

    // Start the service
    service.SetAction("START");
    StartService(service);
  }

  // This should only be added if you want to remove the service when "closing" the app
  protected override void OnDestroy() 
  {
    var service = new Intent(this, typeof(MyForegroundService));
    StopService(service);
  }
}
```
{{% /split %}}
{{< /splitter >}}  