---
title: "Upgrading .Net 4.8 to NET 6 - Aspnetcore + Background Service"
date: 2022-03-05T09:19:40+10:00
draft: true
type: "post"
slug: "upgrading-net48-to-net6-aspnetcore-service"
tags: ["net6"]
---

UP TO HERE: Fix loads of db mappings

<!--more-->  

# TLDR
- Auth hard
- NET 6 good
- Windows Service with net 6 good
- Loads of code deleted, good

# Existing tech stack  
- AspNet MVC
  - Serilog
  - Entity Framework 6
  - Autofac  
  - Signalr (ancillary use, not core functionality)
  - Custom cookie based authentication
  - React frontend (not working on that here)
- Windows Service (Topshelf in .Net 4.8)
- Azure build/deploy
- Running on premises

# Desired Outcome
- Aspnetcore
  - Built in logging
  - EFCore
  - MS DI
  - Signalr
  - Custom cookie based auth (still required, just needs to work)
  - React frontend (not working on that here)
- Windows Service built from aspnetcore hosted service (no Topshelf)
- Azure build/deploy
- Still on premises :(

> I wanted to find out what a near full Microsoft solution looked like.  
> I make no statements on the efficacy or usability of the third party tools objectively, although I must say that subjectively, the MS DI is more understandable and usable than Autofac...😬  

# Planned Upgrade Path  
Use the [Microsoft Upgrade tool]()! 

# Actual Upgrade Path  
I started off rebuilding the projects to use the dotnetcore csproj structure and target Net 6.  
Making a new project and then copying the relevant bits into it in VSCode/text editor seemed the simplest way, and went pretty smoothly.  
I cleaned up a load of things as well, like `AssembyInfo` (although I think the file itself is still supported, the way it was written wasn't, and I wasn't using it)

## Configuration
Moving away from `web.config` and `parameters.xml` to the shiny new `appsettings.json` was also nice and simple, if typing heavy.  
However, I also used it as an opportunity to clean up the structure of the Configuration, grouping settings and features more appropriately.  
This caused hell when I needed to update the pipeline as I forgot to update it, as it was weeks later. I was confused why the setting was there, but not replacing, but it WASN'T there, it needed `Auth.` in front of it...  

## Controllers  
The controllers were previously returning `IHttpActionResult` and I initially just swapped that to it's dotnetcore counterpart `IActionResult`, however I later realised I can just return the actual type in most cases and it'll "just work".  
There were also a few `FromUri` to `FromQuery` changes as well.  
I also had to add explicit `Route` attributes to the actions, although I feel like I missed something there.  
In the end, I think I prefer the explicit approach with the routes. 

## Autofac to MS DI
This was perhaps the most time consuming part. Autofac was pervasive in the old codebase. Very handy at the time, I'm sure, but I couldn't get it working in Net6. It's supported, but I couldn't get my head around it.  
Besides, I wanted as much Microsoft as I could.  
All of the service registration and injection had to change, but nothing too crazy or complicated, except for the generics for mediator queries.  
This finds closed generic implementations based on a name, so usage is: `services.RegisterMultiple(nameof(IQueryExecutor))` and it finds `public class DoThingExecutor : IQueryExecutor<DoThingQuery>` and registers it as transient.  

```c#
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

## Nugets
This was a bit more painful, as I needed to find compatible nuget packages. In most cases, that was just "use the latest version", however some put up more of a fight.  

Most upgraded easily enough, perhaps some minor code and namespace changes, however some of the Microsoft items required new packages from when they split one package into many or when the Windows specific functionality was pulled into their own packages
EXAMPLES

## Logging
Simple, still look pretty nice without serilog.
EXAMPLES

## EFCore  
I've written about this [before]() when upgrading to dotnetcore, but the EFCore migration was fairly simple.  
Loads of namespace changes, some extra unexpected packages (hello Microsoft.Data.SqlClient vs System.Data.SqlClient).  
In general, I liked the process, and I liked the new functionality FOR EXAMPLE

## Signlar 
Almost trivial to setup EXAMPLE

## Authentication
The hardest part was, as expected, the authentication. Not necessarily because I had to make so many changes, but because it's so opaque and hard to understand the documentation.  
There's a lot of "I thought this should work, oh it doesn't even though the docs describe ALMOST this scenario, not quite".
My difficulties were exacerbated by my not especially great understanding of authentication and authorisation in the first place, but the docs didn't help as much as expected.  

The original solution was to use an `IAuthenticationHandler` and implement it directly, but as it's just a cookie, I thought I'd be able to:  
```c#
services.AddAuthentication(config => config.DefaultScheme = "myscheme").AddCookie(config => <some lambda for validation>)
```

but no, that's not how it works.  
I ended up reusing the original handler, changing some types (mostly service resolution), I ended up with this:

{{< split >}}
{{% splitLeft title="Original" %}}
```c#
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
{{% /splitLeft %}}
{{% splitRight title="net 6" %}}
```c#
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
{{% /splitRight %}}
{{< /split >}}  

### The big mad
How does "default" mean "the base that all others will build off"? EXAMPLE

## Windows Service without Topshelf 
Use this nuget, change the pipeline for service registration and starting.