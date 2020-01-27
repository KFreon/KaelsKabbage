---
title: "Migrating to dotnetcore with EFCore"
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
This bit was fairly trivial for the ASPNetCore 2.2 project I had. Update all projects to use dotnetcore 3.1 in the project file.  
<SCREENSHOT>

There were also some simple changes to Startup.cs (in my case, YMMV).
<SCREENSHOT>

The test server and Autofac DI setup also needed adjusting like so:
<SCREENSHOT>

Then update all nugets as required to support the new framework, and that was it!  
That hasn't caused any issues so far. Nice and simple and successful.  

The migration from EFCore 2 -> 3, not so much :(

## Migrating EFCore 2 --> 3
The major sticking point for this is that EFCore 2 silently performed client-side evaluation when it wasn't able to translate a query. This means that you can write pretty much any valid linq query and EFCore would "just do it".  
For us, EF tended to grab tables and pull them back and forth doing fun things to performance and general understanding of when things were going to happen.  

In EFCore 3, this [has been disallowed](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#linq-queries-are-no-longer-evaluated-on-the-client) and queries that can't be translated now give an exception at runtime. Much better!  

The migration was not easy though. The three main errors I got were:   
### Sql cannot be translated
<SCREENSHOT>  
This one is simple enough. Basically the linq you've got has no SQL representation. Might be that you're using functions, or operators that don't have sensible SQL equivalents.  
e.g. `DBTable<type>.Where(x => ValidateItem(x)).ToArray()`  

Validate item can't be translated, and it doesn't seem that EF can inline things well either, so even if that is a super simple check that CAN be translated, it sees a function and gives up.  
Groupby also doesn't tend to behave on the database. I had enough trouble that I just did all groupbys on the client.  

### RelationalProjectionBindingExpressionVisitor
<SCREENSHOT>
These next two are more complicated, and I'm not 100% sure on their meaning.  
I think this one is about projecting navigations out using a `select`, or perhaps more accurately, trying to project a property of a navigation out.  
Doing `navigation.Select(x => x)` seemed to get around it somehow...   

NOTES
Got it when projecting out a navigation like select thing.someNav.Select(t => t.Id). Including doesn't help. Seems to be exacerbated by any query stuff after the select like Skip/Take. Ordering seemed ok.

Not sure if related, Includes break sometimes causing the whole thing to break. i.e. Had an include causing the query to return wrong rows, different counts, etc. Weirdness and hours of confusion.


### NavigationExpandingExpressionVisitor
<SCREENSHOT>
Another complicated one, I think this is something about includes and joining.  
It tended to happen with ;AJLHUASLKDJGHALISGSDFGL


Sometimes these errors specifically identified the part of the query it didn't like, but often I just got a whole blob of expression tree-looking stuff to sort out, so understanding the kinds of things it didn't like helped.    

### DB conversions
This project uses [NodaTime](https://nodatime.org/) to make our lives easier regarding date times and we configured EF to use a normal DateTime column with a converter to a NodaTime type in the C# entity itself.  

EFCore 2 didn't really care much about this since if it couldn't translate it, it'd just do it client side (pulling tables, etc), but now I couldn't use them in anything that was going to be translated to SQL (where, join, groupby, etc).
They seemed ok in the final projection, since it can read the underlying value and perform the conversion clientside.  

This tended to mean that I had to expose the underlying column to the C# model and use that when doing conditional clauses and the main property in the final projection.  

### An Upside  
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
Because EFCore 2 used to pull things into memory, behaviours often change.  
For me, ordering of Includes wasn't behaving consistently. Ok, I need to get around that another way, fair enough.  
Another situation was that we had ordering on an early table join, and because it was clientside (EFCore 2) that ordering was generally being preserved in the final projection. However, now since that doesn't happen anymore, the DB was allowed to do what it liked and the ordering wasn't consistent anymore.  

#### Unit testing
Catch 22 when unit testing EF queries - I had a query that worked fine in EF but failed in the unit test (NSubstitute, etc) because one of the entites  was null, and while SQL could handle that, C# + Linq couldn't.
In the end, there was no way I could make both of them happy, so I had to move to a subcutaneous test that hit a localdb instance.  

### Conclusion
Ultimately we were abusing the frameworks we had and we were allowed to do so in EFCore 2, making the move to EFCore 3 more difficult.  
The actual dotnetcore 2 --> 3 migration was quite nice and simple.  




