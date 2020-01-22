---
title: "Migrating to dotnetcore  With EFCore"
date: 2020-01-14T20:57:48+10:00
draft: true
type: "post"
slug: "migrating-to-dotnetcore3-with-efcore"
tags: ["dotnetcore", "efcore"]
---

(Dotnetcore 3)[] has been out for a little while now, and there were some changes I wanted in the project I was working on, so off I went and gave it a burl.   
Needless to say, there were some issues, but in fairness, it was mostly our fault.  

<!--more-->  

This is predominantly going to be about the migration from EFCore 2 -> 3, since that was by far the biggest pain point.  

# Migrating dotnetcore 2 -> 3
This bit was fairly trivial for the ASPNetCore 2.2 project I had. Update all projects to use dotnetcore 3.1 in the project file.  
<SCREENSHOT>

There were also some simple changes to Startup.cs (in my case, YMMV).
<SCREENSHOT>

The test server and Autofac DI setup also needed adjusting like so:
<SCREENSHOT>

Then update all nugets as required to support the new framework, and that was it!  
That hasn't caused any issues so far. Nice and simple and successful.  

The migration from EFCore 2 -> 3, not so much :(

# Migrating EFCore 2 -> 3
The major sticking point for this is that EFCore 2 silently performed client-side evaluation when it wasn't able to translate a query. This means that you can write pretty much any valid linq query and EFCore would "just do it".  
For us, EF tended to grab tables and pull them back and forth doing fun things to performance and general understanding of when things were going to happen.  

In EFCore 3, this (has been disallowed)[] and queries that can't be translated now give an exception at runtime. Much better!  

The migration was not easy though. The three main errors I got were:   
## Sql cannot be translated
<SCREENSHOT>  
This one is simple enough. Basically the linq you've got has no SQL representation. Might be that you're using functions, or operators that don't have sensible SQL equivalents. 
e.g. `DBTable<type>.Where(x => ValidateItem(x)).ToArray()`  
Validate item can't be translated, and it doesn't seem that EF can inline things well either, so even if that is a super simple check that CAN be translated, it sees a function and gives up.  
Groupby also doesn't tend to behave on the database. I had enough trouble that I just did all groupbys on the client.  

## RelationalProjectionBindingExpressionVisitor
<SCREENSHOT>
These next two are more complicated, and I'm not 100% sure on their meaning.  
I think this one is about projecting navigations out using a `select`, or perhaps more accurately, trying to project a property of a navigation out.  
Doing `navigation.Select(x => x)` seemed to get around it somehow...   

## NavigationExpandingExpressionVisitor
<SCREENSHOT>
Another complicated one, I think this is something about includes and joining.  
It tended to happen with ;AJLHUASLKDJGHALISGSDFGL

Sometimes, the error I got specifically identified the part of the query it didn't like, but often I just got a whole blob of expression tree-looking stuff to sort out.  

## DB conversions
This project uses (NodaTime)[] to make our lives easier, but it turns out they can only be used in the final projection, not the Linq -> Sql, which does make some sense.  
Reading the property with a converter on it was fine, but using it in joins or wheres or anything that required calculation or comparison failed.  
I needed to expose the underlying column as a property that I could use when joining, etc.  
I feel like there's a better way, but I wasn't able to find it in the time allotted. 

Much more thought needs to go into what LINQ is going to do with your query. Sometimes it's not obvious what you're not allowed to do.

Breaking change with database sequences - Need to add `ValueGeneratedNever` to property in entity configuration.

Because EFCore 2 used to pull things into memory, behaviours often change, e.g. Ordering wasn't working for some reason, and I realised that of course it wasn't, it was only one one table join, but in EFCore 2, that would have been the entire dataset, but in 3, it was doing that in the DB, then joining in more tables and coming up with some semi-sorted result.

Catch 22 when unit testing EF queries - had a query that worked fine in EF but failed in the unit test (NSubstitute, etc) because one of the entites  was null, and while SQL could handle that, C# + Linq couldn't.
In the end, there was no way I could make both of them happy, so I had to move to a subcutaneous test :(

We were kinda abusing Linq and the test framework which caused all sorts of issues.


