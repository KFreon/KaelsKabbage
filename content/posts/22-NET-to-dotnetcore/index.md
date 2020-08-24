---
title: "Nancy + Castle + .NET 4.7 to dotnetcore"
date: 2020-08-07T14:44:43+10:00
draft: true
type: "post"
slug: "dotnetcore-nancy-castle"
tags: ["dotnetcore", "nancy"]
---

Nancy was deprecated earlier this year, so I decided to do a full upgrade of a codebase from .NET 4.7 to dotnetcore 3.1.  
It's been a loooong journey...Nancy and Castle have been interesting to migrate away from.  

<!--more-->  

Coming from doing a [dotnetcore 2 -> 3 + EFCore]({{< ref "/posts/18-migrating-to-dotnetcore3-with-efcore/index.md" >}}) upgrade last year, and the Nancy deprecation, I figured it'd be easy to bring this project up to snuff!  
I was wrong.  

# .NET 4.7 --> dotnetcore  
The actual .NET to dotnetcore part wasn't particularly bad. It was essentially:  

- A fair bit of nuget upgrading and fixing the breaking changes there  
- Rewriting most of the Authentication and Authorization logic (partly due to Nancy, but also the new policies, etc)  
- Fixing database `SaveChanges` user stamping which used a thread httpcontext user (which wasn't guaranteed to actually work, and was [removed in dotnetcore](https://docs.microsoft.com/en-us/aspnet/core/migration/claimsprincipal-current?view=aspnetcore-3.1) for that reason)  

I expected the Nuget breakages and had time set aside for those. I also anticipated some of the auth/routing issues, but not as many as I got **(NAAAANCYYYYYY!!!)**  

Auth was fun. I ran into a few issues, but one that stood out was the client was sending a token along with the login (to get some details from the server).  
Since the token was expired, it'd fail auth, and fail getting the details it needed to get new auth, so you couldn't log in!  
Took a while to realise what was going on and why, but in the end, I put a handler above the auth handlers to intercept and suitably redirect/handle those few situations.  

The database stamping was an interesting one.  
The way it used to work was:  

Psuedocode

``` csharp
public override Task SaveChangesAsync() 
{
    var user = Thread.CurrentPrinciple;
    foreach(var entity in ChangeTracker.Entities) 
    {
        entity.LastModifiedBy = user.UserName;
    }
}
```  

Now I pass in the username in during context creation (we use the factory pattern).

``` csharp
// DOTNETCOREDatabaseContextFactory
public void Create() 
{
    return new DOTNETCOREDatabaseContext(services.GetRequiredService<IUserIdentity>().Name);
}

/////////

private readonly string _username;
public DOTNETCOREDatabaseContext(string username) {
    _username = username;
}

public override Task SaveChangesAsync() 
{
    foreach(var entity in ChangeTracker.Entities) 
    {
        entity.LastModifiedBy = _userName;
    }
}
```  

# Castle --> aspnetcore DI  
The built-in DI is pretty nice to work with.  
It has some differences and limitations from the purpose-built DI systems, but they haven't been too hard to work around.  
The main differences I had to deal with were:  

#### Castle can create a container whenever it likes (e.g. tests), whereas this can't   
Since I can't make a container for tests without spinning up a whole TestServer to host it, I had to rewrite those tests to be full unit or integration tests.  

#### Auto-registration doesn't have a built-in replacement  
In Castle, you could register all implementations of a type like: 
``` csharp
Classes.FromThisAssembly()
    .Where(Component.IsInSameNamespaceAs<CsvSerialiser>())
    .WithServiceDefaultInterfaces()
```

Now I've had to make some extension methods to these kinds of things (based on some Stackoverflow items I can't find 😢 )  

``` csharp
 public static void RegisterAsImplementedInterfaces<T>(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
{
    var typesFromAssembly = assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)));
    foreach (var type in typesFromAssembly)
        services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
}

public static void RegisterAsImplementedInterfaces(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime, params Type[] types)
{
    foreach (var type in types)
    {
        var typesFromAssembly = assembly.DefinedTypes.Where(x => x.GetInterfaces().Any(t => t.IsAssignableFrom(type)));
        var genericVersions = assembly.DefinedTypes.Where(x => x.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).Any(t => t.IsAssignableFrom(type)));

        foreach (var assemblyType in genericVersions)
            services.Add(new ServiceDescriptor(assemblyType.ImplementedInterfaces.FirstOrDefault() ?? type, assemblyType, lifetime));

        foreach (var assemblyType in typesFromAssembly)
            services.Add(new ServiceDescriptor(type, assemblyType, lifetime));
    }
}

public static void RegisterAsSelf(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime, params Type[] types)
{
    foreach (var type in types)
    {
        var typesFromAssembly = assembly.DefinedTypes.Where(x => x.GetInterfaces().Any(t => t.IsAssignableFrom(type)));
        var genericVersions = assembly.DefinedTypes.Where(x => x.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).Any(t => t.IsAssignableFrom(type)));

        foreach (var assemblyType in genericVersions)
            services.Add(new ServiceDescriptor(assemblyType, assemblyType, lifetime));

        foreach (var assemblyType in typesFromAssembly)
            services.Add(new ServiceDescriptor(assemblyType, assemblyType, lifetime));
    }
}
```

# Nancy  
This was the guts of the change, but there isn't much to write about because it was mostly "rewrite the tests and controllers and user hydration".  
Users were magically built up using some fancy Nancy toys, now it's a scoped DI service that I can just resolve and use in the normal service way instead of semi-arbitrarily.  

> Note that the MAIN way to get this is via [controller action injection](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-3.1#action-injection-with-fromservices)  
```
[HttpGet]
public async Task<IHttpActionResult> Get([FromServices] IUserIdentity user)
```

Controllers were modules, and built completely differently.   

- Now inherit from `ControllerBase`, and have the `[ApiController]` on it to get some [free goodies](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1#apicontroller-attribute).  
- Auth isn't handled by the controller constructors anymore, and is either living in `startup.cs` as policies and whatnot, and/or as Attributes on the controller.  
- Routes were setup in the constructor, now moved to methods with attributes indicating child routes and HTTP methods.  
- Looooots of `dynamic` returns, ripped all that out and moved to the built in methods like `Ok(some object to return)` or `Created(new ID to return)`  
- Tests rewritten to use the new [`TestServer`](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1) in-memory server hosting instead of what Nancy was doing (similar...?)

Early in the migration, I was getting `415 Unsupported Media Type` when testing the migrated controllers.  
There was no way that was true, so digging lead me to wonder if the binding engine was struggling with some of the types.  
Maybe that would cause it too, but no, turns out it was just missing the bindings.  
Nancy binds to HTTP Action parameters like this: `var dto = this.Bind<SomeDto>()` and some black magic made it work.  
Now, I can just take it in as a parameter to the action (sometimes requiring extra attributes): `public async Task Post([FromBody] SomeDto dto)`  