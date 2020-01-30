---
title: "Migrating to dotnetcore 3.1 (mostly EFCore)"
date: 2020-01-14T20:57:48+10:00
draft: true
type: "post"
slug: "migrating-to-dotnetcore3-with-efcore"
tags: ["dotnetcore", "efcore"]
---

[Dotnetcore 3](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-3-0) has been out for a little while now, and there were some changes I wanted in the project I was working on, so off I went and gave it a burl.   
Needless to say, there were some issues, but in fairness, it was mostly our fault.  

<!--more-->  

This is predominantly going to be about the migration from EFCore 2 --> 3, since that was by far the biggest pain point.  

## Migrating dotnetcore 2 --> 3
The framework migration was fairly trivial for the ASPNetCore 2.2 project I had.  
Update all projects to use dotnetcore 3.1 in the project file.  
`<TargetFramework>netcoreapp2.1</TargetFramework>` --> `<TargetFramework>netcoreapp3.1</TargetFramework>`  

There were also some simple changes to Program.cs and Startup.cs (in my case, YMMV).  
**Program.cs**   

{{% split %}}
{{% splitLeft title="Original" %}}
``` csharp
var webHostBuilder = WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration(builder => additionalConfig?.Invoke(builder))
  .UseContentRoot(appRootPath)
  .ConfigureServices(services => services.AddAutofac())
  .UseStartup<TStartup>()
  .UseSerilog();
```
{{% /splitLeft %}}
{{% splitRight title="dotnetcore3" %}}
``` csharp
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
``` csharp
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<LoggingMiddleware>();
app.UseMvc();
```
{{% /splitLeft %}}
{{% splitRight title="dotnetcore3" %}}
``` csharp
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
``` csharp
Server = new TestServer(Program
  .GetWebHostBuilder<TestServerStartup<TestUserIdentity>>(appRootPath, null, TestConfiguration.AddTestConfig));
```
{{% /splitLeft %}}
{{% splitRight title="dotnetcore3" %}}
``` csharp
var server = await Program
  .GetWebHostBuilder<TestServerStartup<TestUserIdentity>>(appRootPath, null, TestConfiguration.AddTestConfig)
  .ConfigureWebHost(webBuilder => webBuilder.UseTestServer()).StartAsync();

Server = server.GetTestServer();
```
{{% /splitRight %}}
{{% /split %}}  

Then in the tests, we would resolve services with `Server.Host.Services.GetService`, which now drops the `Host` to just `Server.Services.GetService`  

Then update all nugets as required to support the new framework, and that was it!  
That hasn't caused any issues so far. Nice and simple and successful.  

The migration from EFCore 2 --> 3, not so much :(

## Migrating EFCore 2 --> 3
The major sticking point for this is that EFCore 2 silently performed client-side evaluation when it wasn't able to translate a query. This means that you can write pretty much any valid linq query and EFCore would "just do it".  
For us, EF tended to grab tables and pull them back or make `n+1` subqueries,  doing fun things to performance and general understanding of when things were going to happen.  

In EFCore 3, this [has been disallowed](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#linq-queries-are-no-longer-evaluated-on-the-client) and queries that can't be translated now give an exception at runtime. Much better!  

The migration was not easy though. The three main errors I got were:   
### Sql cannot be translated
Obviously this was the main one. Any linq you write now has to be able to be translated. That doesn't mean you can't use complex types and programming structures, as EF can translate some things (sometimes with your help)  
Functions can sometimes be translated (not not often, usually can't do `DBTable<type>.Where(x => ValidateItem(x)).ToArray()`), and as long as you have a [value converter](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions), you can use those properties too.  

You can't get away with things like `GroupJoin` as that doesn't have a representation in SQL, nor can you do a `GroupBy` and get the key with grouped children like in linq.  
More explicitly:  
If you have a type: `{Guid Id, string Name, int Price}`, and you want to have a result: `{Id, Name, List<int> Prices}`.  

`SomeArray.GroupBy(x => new { x.Id, x.Name }).ToList()` will fail to translate in EF as you haven't told it how to aggregate the price.  
You can do `SomeArray.ToList().GroupBy(x => new { x.Id, x.Name })` to do it client-side.   

For clarity, you CAN do `SomeArray.GroupBy(x => new { x.Id, x.Name }).Select(x => new { x.Key.Id, x.Key.Name, MaxPrice = x.Max(t => t.Price) }).ToList()` to get a list with the maximum price, just not all the prices.  

`SelectMany` that wasn't the last thing in the query also didn't behave, and sometimes even when it was the last thing...  

### RelationalProjectionBindingExpressionVisitor
These next two are more complicated, and I'm not 100% sure on their meaning.  
I tended to get it doing `thing.navigation.Select(x => x.Id)` or even just `thing.Navigation`, or perhaps more accurately, trying to project a property of a navigation out.  
Doing `thing.navigation.Select(x => x)` seemed to get around it somehow...   

### NavigationExpandingExpressionVisitor
Another complicated one, I think this is something about includes and joining, but how it actually differes from the above, I'm not 100%. They're both something about navigations/includes.  

Sometimes these errors specifically identified the part of the query it didn't like, but often I just got a whole blob of expression tree-looking stuff to sort out, so understanding the kinds of things it didn't like helped.    

#### An Upside  
One upside to these changes is that you need to put much more thought into what LINQ is going to do with your query. You can't do whatever you want and get horrible queries without knowing.


### Some more downsides  
#### Database Sequences and generated/sourced Ids
There was a breaking change regarding Database sequences and DB generated ID's. If you have an entity that has a DB generated or sourced Id as a PK AND you assign it one in C# anyway, EF now assumes the row exists and tracks it as "Modified" instead of "Added".  
This was a problem for me, and I needed to add `.ValueGeneratedNever` to the property in entity configuration. I couldn't add it to the `key` definition, but I was allowed to also have a property definition I could set it on.  
i.e.  

```
config.HasKey(x => x.Id);
config.Property(x => x.Id).ValueGeneratedNever();
```

#### Ordering differences
Because EFCore 2 used to pull things into memory, behaviours were C# like. Database ordering isn't always the same :(   
For me, ordering of Includes wasn't behaving consistently. Ok, I need to get around that another way, fair enough.  
Another situation was that we had ordering on an early table join, which was working fine in EFCore 2 because it was clientside.  
EFCore 3 disregarded that OR at least didn't preserve the ordering indicated after further operations, like joins, which is fair enough.  

#### Unit testing
Catch 22 when unit testing EF queries - I had a query that worked fine in EF but failed in the unit test (NSubstitute, etc) because one of the entites  was null, and while SQL could handle that, C# + Linq couldn't.
In the end, there was no way I could make both of them happy, so I had to move to a subcutaneous test that hit a localdb instance.  

### Conclusion
Ultimately we were abusing the frameworks we had and we were allowed to do so in EFCore 2, making the move to EFCore 3 more difficult.  
The actual dotnetcore 2 --> 3 migration was quite nice and simple.  




