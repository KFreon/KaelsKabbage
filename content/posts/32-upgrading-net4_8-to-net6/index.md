---
title: "Upgrading .NET 4.8 to NET 6: Aspnetcore Auth + Background Service"
date: 2022-05-06T10:00:40+10:00
type: "post"
slug: "net48-to-net6-aspnetcore-auth-service"
tags: ["net6"]
---

I recently upgraded a project from NET 4.8 to NET 6, and the most interesting parts were Autofac to MS DI, Auth, and the Windows Service.  
It went fairly well!

<!--more-->  

# TLDR
- Auth is hard
- Windows Service is easier

# Scope
We'll just 
Straightforward changes:
- Controller migration (previously returning `IHttpActionResult`, moved to explicit return types)
- Autofac --> Microsoft DI (details [below](#moving-from-autofac-to-ms-di))
- Nuget upgrades (some frustrations with similar namespaces)
- Logging
- EFCore (`Microsoft.Data.SqlClient` vs `System.Data.SqlClient`)
  - MAKE SURE you use the new `Microsoft.Data.SqlClient`. Autocompletion sometimes picks the old one...
- Signalr

## Moving from Autofac to MS DI
This was perhaps the most time consuming part of the migration. Autofac was pervasive in the old codebase. Very handy at the time, I'm sure, but I couldn't get it working in Net6. It's supported, but I couldn't get my head around it.  
Besides, I wanted to play with the Microsoft solution.  
Aside from the standard injection and registration, the only main change is for registering the generics for mediator queries.  
The following method finds closed generic implementations based on a name and registers it as Transient
(usage is: `services.RegisterMultiple(nameof(IQueryExecutor))` and it finds `public class DoThingExecutor : IQueryExecutor<DoThingQuery>`)  

```cs
private static void RegisterMultiple(this IServiceCollection builder, string name)
{
    var apiTypes = typeof(Program)
        .GetTypeInfo()
        .Assembly
        .GetTypes();

    var domainTypes = typeof(IQueryExecutor)  // Just for assembly
        .GetTypeInfo()
        .Assembly
        .GetTypes();

    var queryHandlers = apiTypes.Concat(domainTypes)
        .Where(x => !x.IsGenericType)
        .Where(x => x.GetInterfaces().Any(t => t.Name.StartsWith(name)))
        .Select(x => new { Type = x, Generic = x.GetInterfaces().FirstOrDefault() })
        .Where(x => x is not null);

    foreach (var handler in queryHandlers)
    {
        builder.AddTransient(handler.Generic, handler.Type);
    }
}
```

## Authentication
The hardest part was, as expected, the authentication. Not necessarily because I had to make so many changes, but because I found the documentation opaque and hard to understand.  
There's a lot of "I thought this should work, but it doesn't".  
My difficulties were exacerbated by my not especially great understanding of authentication and authorisation in the first place, but the docs didn't help as much as expected.  

The original solution implemented `IAuthenticationHandler` and implement it directly, but I figured I'd be able to:  
```cs
services.AddAuthentication(config => config.DefaultScheme = "myscheme").AddCookie(config => <some lambda for validation>)
```

but no, that's not how it works.  
I ended up reusing the original handler, changing some types (mostly service resolution), I ended up with this:

{{% splitter %}}
{{% split side=left title="Original" %}}
```cs
public class MyAuthHandler : IAuthenticationHandler
{
    public Task<AuthenticateResult> AuthenticateAsync()
    {
        var sessionStore = context.OwinContext.GetAutofacLifetimeScope().Resolve<ICurrentSessionStore>();
        var validationResult = await sessionStore.Validate();

        if (validationResult.Succeeded)
        {
            var ticket = new AuthenticationTicket(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, validationResult.UserEmail) }, "Bearer"),
                new AuthenticationProperties());

            context.SetTicket(ticket);
        }

        var session = SessionCookieHelper.GetFromCookie(context.OwinContext);

        if (session != null)
        {
            var sessionStore = context.OwinContext
                .GetAutofacLifetimeScope().Resolve<ICurrentSessionStore>();
            sessionStore.Set(session);
            context.Token = session.AccessToken;
        }
    }

    public Task ChallengeAsync(AuthenticationProperties properties)
    {
        throw new System.NotImplementedException();
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        throw new System.NotImplementedException();
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        throw new System.NotImplementedException();
    }
}

public static class AuthConfig
{

    public static void ConfigureAuth(this IAppBuilder app)
    {
        app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
        {
            Provider = new OAuthBearerAuthenticationProvider
            {
                OnRequestToken = context =>
                {
                    var session = SessionCookieHelper.GetFromCookie(context.OwinContext);

                    if (session != null)
                    {
                        var sessionStore = context.OwinContext
                            .GetAutofacLifetimeScope().Resolve<ICurrentSessionStore>();
                        sessionStore.Set(session);
                        context.Token = session.AccessToken;
                    }

                    return Task.CompletedTask;
                }
            },
            AccessTokenProvider = new MyTokenProvider(),
        });
    }
}
```
{{% /split %}}
{{% split side=right title="NET 6" %}}
```cs
public class MyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public const string MyAuthScheme = "MyAuthScheme";
}

public class MyAuthHandler : AuthenticationHandler<MyAuthSchemeOptions>
{
    public MyAuthHandler(IOptionsMonitor<MyAuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var session = SessionCookieHelper.GetFromCookie(Request.Cookies);

        // This is a custom thing we're doing, that is almost certainly not required, but I don't have time to fix it
        var sessionStore = Context.RequestServices.GetRequiredService<ICurrentSessionStore>();
        sessionStore.Set(session);

        var validationResult = await sessionStore.Validate();

        if (validationResult.Succeeded)
        {
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, validationResult.UserEmail) }, "Bearer");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        else
        {
            return AuthenticateResult.Fail("Error during auth");
        }
    }
}
```
{{% /split %}}
{{< /splitter >}}  

### The Big Mad
I have a primary authorisation policy, and a couple of others for single endpoint policies for third parties.  
I wanted to have the primary policy used by default unless something more specific is applied by attribute.  
Apparently, the `DefaultPolicy` is the base policy that all others build off, not the one to use if all others fail.  
What I wanted was actually the `FallbackPolicy`.  

```cs
services.AddAuthentication(d => d.DefaultScheme = MyAuthSchemeOptions.MyAuthScheme)
    .AddScheme<MyAuthSchemeOptions, MyAuthHander>(MyAuthSchemeOptions.MyAuthScheme, opts => { })
    .AddScheme<OtherSchemeOptions, OtherSchemeHandler>(OtherSchemeOptions.OtherScheme, opts => { })
    .AddScheme<AnotherSchemeOptions, AnotherSchemeHandler>(AnotherSchemeOptions.AnotherScheme, opts => { });

services.AddAuthorization(x =>
{
    x.AddPolicy(OtherSchemeOptions.OtherScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(OtherSchemeOptions.OtherScheme);
        policy.RequireAuthenticatedUser();
    });

    x.AddPolicy(AnotherSchemeOptions.AnotherScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(AnotherSchemeOptions.AnotherScheme);
        policy.RequireAuthenticatedUser();
    });

    // Fallback here is what I think of as default.
    // It turns out "defaultPolicy" is applied across the board and added to, not overridden.
    x.FallbackPolicy = new AuthorizationPolicyBuilder(MyAuthSchemeOptions.MyAuthScheme)
        .RequireAuthenticatedUser()
        .AddRequirements(new SomeRequirement())
        .Build();
});
```

In retrospect, it does somewhat makes sense, but I was confused and quite annoyed for having spent so much debugging time on it.  

## Windows Service without Topshelf 
Essentially, I just had to:
- Remove all Topshelf setup
- Use [this](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.WindowsServices) Nuget
- Add `.UseWindowsService()` at the end of the `IHostBuilder` just before the `.Build()`

```cs
var host = Host.CreateDefaultBuilder(args)
    // Other configuration and setup
    .UseWindowsService()
    .Build();

host.Run();

```

The Release Pipeline also required changes from Topshelf installs to:


{{% splitter %}}
{{% split side=left title="Uninstall Old Service" %}}
``` bash
sc stop "$(Application)"
timeout /t 5 /nobreak > NUL
sc delete "$(Application)"
```
{{% /split %}}
{{% split side=right title="Install New Service" %}}
``` bash
sc create "$(Application)" binPath=$(BackgroundServicePath)/BackgroundTasks.exe
sc config "$(Application)" start=auto obj=$(ServerAdminUsername) password=$(ServerAdminPassword)
sc start "$(Application)"
```
{{% /split %}}
{{< /splitter >}}  


# Conclusion
All in all, the migration went well, but it did take quite a while.  
The changes in NET 6 allowed me to delete whole swaths of code, and simplify the auth, which was nice.  

`85 files deleted` :)