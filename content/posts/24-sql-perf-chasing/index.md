---
title: "SQL Performance Chasing"
date: 2020-09-28T12:03:07+10:00
draft: false
type: "post"
slug: "24-sql-perf-chasing"
tags: ["sql", "perf"]
---

We're moving a client to Azure from on premises and there were some performance hurdles.  
They weren't unexpected, and nearly all of them were database related.  

<!--more-->  

# Contents  

{{% toc %}}

# N+1  
We had a data structure like:  

- Vehicle  
    - Part  

Sometimes we'd get a list of vehicles then `foreach` vehicle, concat the part name and number (simplistic example)  
This seems reasonable, but let's say we didn't pull the Parts in with the vehicles? What if it wasn't that simple?   


{{% split %}}
{{% splitLeft title="Bad versions" %}}
``` csharp
foreach(var vehicle in dbContext.Table<Vehicle>())
{
    var vehiclePart = dbContext.Table<Part>().First(p => vehicle.Parts.Contains(p));

    // Or you have EF lazy loading and just access the Parts.
}
```
{{% /splitLeft %}}
{{% splitRight title="Good versions" %}}
``` csharp
// Can be good if the includes are small, more on this later
dbContext.Table<Vehicle>().Include(v => v.Parts)

OR

// Fetch one set of data first, then resuse later.
// Good, especially when there's not a relation to use the Include.
var parts = dbContext.Table<Parts>().ToDictionary(x => x.Id);
foreach(var vehicle in dbContext.Table<Vehicles>())
{
    // Do things here like:
    var vehiclePart = parts[vehicle.Id];
}
```
{{% /splitRight %}}
{{% /split %}}  

There were some less obvious ones like [Mediatr](https://github.com/jbogard/MediatR) queries that would be run on each iteration which were poorly optimised or unnecessary.  

# Reuse  
Similar to the above, there were plenty of opportunitites to reuse data we'd already fetched.  
e.g.

``` csharp
foreach(var part in dbContext.Table<Part>())
{
    var isBig = dbContext.Table<PartType>().Where(x => x.Id == part.Id && x.IsBig);
}

BETTER

var allPartTypes = dbContext.Table<PartType>().Select(x => new { Id = x.Id, IsBig = x.IsBig }).ToDictionary(x => x.Id);
foreach(var part in dbContext.Table<Part>())
{
    var isBig = allPartTypes.Where(x => x.Id == part.Id && x.IsBig);
}
```

In many situations, we'd end up going through all the partTypes anyway, so we'd use the whole table.  

Less obvious versions of this were again related to Mediatr queries where we could pass extra info so we didn't have to do the same thing inside the query on multiple loops.  
e.g. `mediatr.Send(new GetVehicles(allPartTypes))` instead of fetching the part types every loop.

# Drowning  
Includes caused a few issues.  
We'd join on five tables, each would be including... You see where I'm going.  
Turns out returning 2M rows when you only need 10 isn't great.  
Filtering with `.Where`, pagination, and splitting queries were the solutions to this.

Another thing, don't return unnecessary info where possible.  
We'd return the whole object instead of the ID and dateCreated which were all we needed.  

# Big Bois  
Sometimes there's just not a lot you can do. Some queries are just big.  
We made some indexes, optimised some returns, added some filtering, but it's still slow and probably always will be.  

# Chatting  
Lastly, we had some chatty requests.  
We'd hit the database six times for something that could be done in one.  
Dashboards were a common occurrance of this issue; Each tile would hit the database at least once.  
Now there's some nice Big Boi SQL that does it all, but ultimately it's more performant than the 12 little ones.

