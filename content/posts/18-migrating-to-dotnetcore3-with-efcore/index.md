---
title: "Migrating to dotnetcore  With EFCore"
date: 2020-01-14T20:57:48+10:00
draft: true
type: "post"
slug: "migrating-to-dotnetcore3-with-efcore"
tags: ["dotnetcore", "efcore"]
---



<!--more-->  


sometimes you get a decent error report e.g. this chucnk of linq can't be converted or has a navigation expanding error, but rare.

Some startup items need updating.

Also had many issues with unit tests and the new test setup (easy to setup, we'd just been abusing it)
Test server and DI setup also needed changing - simple, just necessary.

DB converters can only be used in the final projections (makes sense), so we needed to expose actual values and do most of the previously DB conversions on the Database.

Much more thought needs to go into what LINQ is going to do with your query. Sometimes it's not obvious what you're not allowed to do.

The three things: Sql translation (cannot be translated), entity access (RelationalProjectionBindingExpressionVisitor), and another entity access (NavigationExpandingExpressionVisitor)
I think the second one is issue with children when accessing it and projecting out, and the last one is about includes and joining (not really sure, and it's hard to know)

Sometimes when projecting out an included navigation, it fails, and can be beaten by <navigation>.Select(x => x). Unsure why this works, maybe just type coercion?

Breaking change with database sequences - Need to add `ValueGeneratedNever` to property in entity configuration.

Because EFCore 2 used to pull things into memory, behaviours often change, e.g. Ordering wasn't working for some reason, and I realised that of course it wasn't, it was only one one table join, but in EFCore 2, that would have been the entire dataset, but in 3, it was doing that in the DB, then joining in more tables and coming up with some semi-sorted result.