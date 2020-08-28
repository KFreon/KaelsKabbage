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

# tl;dr  
- .NET --> dotnetcore was fairly simple ----- *(nugets, etc)*  
- EF6 on dotnetcore was a bit trickier, but simple in the end ----- *(connection strings, config)*  
- Castle --> aspnetcore DI was time consuming, but not terrible  ----- *(looots to do)* 
- Nancy --> WebApi was painful, and time consuming, but ultimately doable ----- *(tests, controllers, auth, users, etc)*   

# .NET 4.7 --> dotnetcore  
The actual .NET to dotnetcore part wasn't particularly bad. It was essentially:  

- A fair bit of nuget upgrading and fixing the breaking changes there  
- Rewriting most of the Authentication and Authorization logic (partly due to Nancy, but also the new policies, etc)  
- Fixing database `SaveChanges` user stamping which used a thread httpcontext user (which wasn't guaranteed to actually work, and was [removed in dotnetcore](https://docs.microsoft.com/en-us/aspnet/core/migration/claimsprincipal-current?view=aspnetcore-3.1) for that reason)  

I expected the Nuget breakages and had time set aside for those. I also anticipated some of the auth/routing issues, but not as many as I got **(Fist shaking -- NAAAANCYYYYYY!!!)**  

The database audit stamping was an interesting problem; See the way it used to work was:  

**Psuedocode**

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

# EF6 on dotnetcore  
EF6 doesn't really like dotnetcore, or at least it didn't like me and dotnetcore.  
I couldn't get database migration initialisers working, which meant that I couldn't do migrations the EF way anymore.  
Boo... but DBUP = YAY!  
I had to set the initialiser to null: `database.SetInitialiser<MyContext>(null)` or it would try and do some default behaviours (which it couldn't)  
I THINK the issue with the initialisers is due to the config not being available to EF6. It seemed to be trying to read the connection string out of config, but not `appsettings.json` (`app.config` or `web.config`), but failing since they don't exist.  
It was doing this internally, and I couldn't figure out how to stop it.  

# Castle --> aspnetcore DI  
The built-in DI is pretty nice to work with.  
It has some differences and limitations from the purpose-built DI systems, but they haven't been too hard to work around.  
The main differences I had to deal with were:  

- Castle can create a container whenever it likes (e.g. tests), whereas this can't   
    - Since I can't make a container for tests without spinning up a whole TestServer to host it, I had to rewrite those tests to be full unit or integration tests.  

- Auto-registration doesn't have a built-in replacement  
    - In Castle, you could register all implementations of a type like:  
    ``` 
    Classes.FromThisAssembly()
        .Where(Component.IsInSameNamespaceAs<CsvSerialiser>())
        .WithServiceDefaultInterfaces()
    ```

Now I've had to make some extension methods to these kinds of things (based on some Stackoverflow items I can't find 😢 )  

``` csharp
// I've just realised this name is wrong. It's more like "Register all implementations of interfaces"
// Note the "genericVersions" bit. I believe that's getting the concrete type of a generic and registering that, but this is all new to me.
// e.g. IPet with Dog implementation, Dog is registered as IPet.
// Generic e.g. IPet<FourLegs> with Cat: IPet<FourLegs> implementation, Cat is registered as IPet<FourLegs>
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

// e.g. IPet with Dog implementation, Dog is registered as Dog, not IPet.
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
This was the guts of the changes, but there isn't much to write about because it was mostly "rewrite the tests and controllers and user hydration".  
Users were magically built up using some fancy Nancy toys, now it's a scoped DI service that I can just resolve and use in the normal service way instead of semi-arbitrarily. It's scoped, because then it has access to the HttpRequest pipeline, and it can perform the necessary transforms on `HttpContext.User`. 

> Note that the MAIN way I use this scoped service is via [controller action injection](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-3.1#action-injection-with-fromservices)  
```
[HttpGet]
public async Task<IHttpActionResult> Get([FromServices] IUserIdentity user)
```  
In Nancy, it was "resolved" wherever, and was either in context and set, or out of context and null.  

Controllers were modules, built completely differently, and thus had to be rewritten.  

- Now inherit from `ControllerBase`, and have the `[ApiController]` on it to get some [free goodies](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1#apicontroller-attribute).  
- Nancy had auth constraints in the constructor. Now either lives in `startup.cs` as policies and whatnot, and/or as Attributes on the controller.  
- Routes were setup in the constructor, now moved to methods with attributes indicating child routes and HTTP methods.  
- Looooots of `dynamic` returns, ripped all that out and moved to the built in methods like `Ok(some object to return)` or `Created(new ID to return)`  
- Tests rewritten to use the new [`TestServer`](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1) in-memory server hosting instead of what Nancy was doing (similar...?)

# Wrap up  
It hasn't been a particularly hard journey, just a time consuming one.  
I had to rewrite a lot of stuff, then fix the tests to make sure I haven't broken anything.  
There was also a lot about Auth (both of them) that I didn't know, and took a fair bit of fiddling to get them working.  
Ultimately though, with the Nancy deprecation, a bunch of rewriting was required anyway, so I think it was worth :)  