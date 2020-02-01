---
title: "Migrating to dotnetcore 3.1 (mostly EFCore)"
date: 2020-01-31T20:57:48+10:00
type: "post"
slug: "migrating-to-dotnetcore3-with-efcore"
tags: ["dotnetcore", "Entity Framework"]
---

[Dotnetcore 3](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-3-0) has been out for a little while now, and there were some changes I wanted in the project I was working on, so off I went and gave it a burl.   
Needless to say, there were some issues, but in fairness, some of it was our fault.  

<!--more-->  

This is predominantly going to be about the migration from EFCore 2 --> 3, since that was by far the biggest pain point.  

# TL;DR  
Dotnetcore 2 --> 3 is easy.  
EFCore 2 --> 3 is harder.  
Need to think about how the Linq will translate into SQL.  
**Can't really do:**    

- `DBTable<type>.Where(x => ValidateItem(x)).ToArray()`
- GroupJoin  
- SelectMany  
- GroupBy


## Migrating dotnetcore 2 --> 3
The framework migration was fairly trivial for the ASPNetCore 2.2 project I had.  
Update all projects to use dotnetcore 3.1 in the project file.  
`<TargetFramework>netcoreapp2.1</TargetFramework>` --> `<TargetFramework>netcoreapp3.1</TargetFramework>`  
Easy as that!  

There were also some simple changes to Program.cs and Startup.cs (in my case, YMMV).  
**Program.cs**   

{{% split %}}
{{% splitLeft title="Original" %}}
```go
var webHostBuilder = WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration(builder => additionalConfig?.Invoke(builder))
  .UseContentRoot(appRootPath)
  .ConfigureServices(services => services.AddAutofac())
  .UseStartup<TStartup>()
  .UseSerilog();
```
{{% /splitLeft %}}
{{% splitRight title="Dotnetcore3" %}}
```go
var webHostBuilder = Host.CreateDefaultBuilder(args)
  .UseServiceProviderFactory(new AutofacServiceProviderFactory())
  .UseSerilog()
  .ConfigureWebHostDefaults(webBuilder =>
  {
      webBuilder.ConfigureAppConfiguration(builder => additionalConfig?.Invoke(builder))
          .UseContentRoot(appRootPath)
          .UseStartup<TStartup>();
  });
```
{{% /splitRight %}}
{{% /split %}}     
Note the different method of registering Autofac.  

**Startup.cs**  
In `ConfigureServices`, `UseMvc` changed to `AddControllers`. As I understand it, there can be a few ways to configure that method, but in my case that's all I needed (basic endpoint configuration)  
`Configure` was getting an `IHostingEnvironment`, that's now `IWebHostEnvironment`.  
Also:  
{{% split %}}
{{% splitLeft title="Original" %}}
```go
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<LoggingMiddleware>();
app.UseMvc();
```
{{% /splitLeft %}}
{{% splitRight title="Dotnetcore3" %}}
```go
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<LoggingMiddleware>();
app.UseAuthorization();  <-- Added
app.UseEndpoints(endpoints => endpoints.MapControllers());  <-- instead of UseMvc
```
{{% /splitRight %}}
{{% /split %}}  

Test config needed adjusting: 
{{% split %}}
{{% splitLeft title="Original" %}}
```go
Server = new TestServer(Program
  .GetWebHostBuilder<TestServerStartup<TestUserIdentity>>(appRootPath, null, TestConfiguration.AddTestConfig));
```
{{% /splitLeft %}}
{{% splitRight title="Dotnetcore3" %}}
```go
var server = await Program
  .GetWebHostBuilder<TestServerStartup<TestUserIdentity>>(appRootPath, null, TestConfiguration.AddTestConfig)
  .ConfigureWebHost(webBuilder => webBuilder.UseTestServer()).StartAsync();

Server = server.GetTestServer();
```
{{% /splitRight %}}
{{% /split %}}  

In the tests, we would resolve services with `Server.Host.Services.GetService`, which now drops the `Host` to just `Server.Services.GetService`  

Update all nugets as required to support the new framework, and that was it!  
That hasn't caused any issues so far. Nice and simple and successful.  

The migration from EFCore 2 --> 3, not so much :(

## Migrating EFCore 2 --> 3
The major sticking point is that EFCore 2 silently performed client-side evaluation when it wasn't able to translate a query. This means that you could write pretty much any valid linq query and EFCore would "just do it".  
For us, EFCore tended to grab tables and pull them back or make `n+1` subqueries, doing fun things to performance and general understanding of when things were going to happen.  

In EFCore 3, this [has been disallowed](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#linq-queries-are-no-longer-evaluated-on-the-client) and queries that can't be translated now give an exception at runtime. Much better!  
EFCore now operates in a more SQL fashion. You can't write any Linq and expect it to "just work". For example, `GroupJoin` doesn't really exist in SQL, so EFCore won't translate it.  
This is good news! No more weird query behaviours due to the EFCore black box (well less of them)  

The migration was not easy though. The three main errors I got were:   

- Sql cannot be translated  
- RelationalProjectionBindingExpressionVisitor  
- NavigationExpandingExpressionVisitor  

The first is easy to understand, but the latter two are a bit of a mystery for me, but they both pertain to operations performed on navigation properties.  

**Essentially, you can't do the following anymore:**  

- `DBTable<type>.Where(x => ValidateItem(x)).ToArray()` (Generally speaking. Sometimes, if the function is super simple, or an Expression, it can be translated.)  
- `Table.GroupJoin(OtherTable)`  
- `Table.SelectMany(x => x.Property)`  
- `Table.GroupBy(x => x)` where x is a complex type (non-sql type) e.g. x could be a Guid, int, etc, but not an entity. More on this later.  

**I also had troubles with things:** 

- `FinalProjection.Select(x => x.Navigation)`  
- `Table.Where(x => x.Navigation.Id == 6)`   

These often resulted in the `Relational` or `Navigation` errors from above. They could sometimes be bypassed with `x.Navigation.Select(t => t)`, although I'm unsure why.   

- Database Sequences and generated/sourced Ids

**A quick aside**, [value converters](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions) DO still work in these queries. I believed they didn't for some time, and went around exposing all the underlying columns.

### The GroupBy Problem  
One of the most frustrating items for us was that `GroupBy` didn't work anymore. We did a lot of grouping, then ordering, then returning data.  
Although it wasn't being done in the database, it was still doing ok compared to massive cross multiplication leading to millions of rows that I got with my first attempt to rewrite this query.  
Now we usually do all our grouping, ordering, etc but get the ID's only, so we can query later to get the rest of the navigations etc. This cuts down the number of joins required, and thus the number of rows that have to be returned.  

An example of the `GroupBy` problem:  

If you have a type: `{Guid Id, string Name, int Price}`, and you want to have a result: `{Id, Name, List<int> Prices}`.  

```go  
SomeArray
  .GroupBy(x => new { x.Id, x.Name })
  .ToList();  // Will fail to translate in EFCore 3 as you haven't told it how to aggregate the price.  

SomeArray
  .ToList()
  .GroupBy(x => new { x.Id, x.Name }); // Could be used to do it client-side.  

// This can done to group by the ID and Name to get them and the MAX price, just not ALL the prices
SomeArray
  .GroupBy(x => new { x.Id, x.Name })
  .Select(x => new { x.Key.Id, x.Key.Name, MaxPrice = x.Max(t => t.Price) })
  .ToList();
``` 

### Database Sequences and generated/sourced Ids
There was a breaking change regarding Database sequences and DB generated ID's. If you have an entity that has a DB generated or sourced Id as a PK AND you assign it one in C# anyway, EF now assumes the row exists and tracks it as `Modified` instead of `Added`.  
This was a problem for me, and I needed to add `.ValueGeneratedNever` to the property in entity configuration. I couldn't add it to the `key` definition, but I was allowed to also have a property definition I could set it on.  
i.e.  

```go
config.HasKey(x => x.Id);
config.Property(x => x.Id).ValueGeneratedNever();
```

#### Ordering differences
Because EFCore 2 used to pull things into memory, behaviours were C# like. Database ordering isn't always the same :(   
For me, ordering of Includes wasn't behaving consistently. Ok, I need to get around that another way, fair enough.  
Another situation was that we had ordering on an early table join, which was working fine in EFCore 2 because it was clientside.  
EFCore 3 disregarded that OR at least didn't preserve the ordering indicated after further operations, like joins, which is fair enough.  
It also seems that ordering on a nullable field wasn't being done correctly in EFCore 2x, and resulted in nulls spread throughout the rest of the values. EFCore 3 has fixed that.  

#### Unit testing
Catch 22 when unit testing EF queries - I had a query that worked fine in EF but failed in the unit test (NSubstitute, etc) because one of the entites  was null, and while SQL could handle that, C# + Linq couldn't.
In the end, there was no way I could make both of them happy, so I had to move to a subcutaneous test that hit a localdb instance.  

### Conclusion
Ultimately we were abusing the frameworks we had and we were allowed to do so in EFCore 2, making the move to EFCore 3 more difficult.  
The actual dotnetcore 2 --> 3 migration was quite nice and simple.  