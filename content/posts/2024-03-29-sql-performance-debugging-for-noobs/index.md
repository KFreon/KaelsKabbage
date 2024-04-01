---
title: "Sql Performance Debugging for noobs"
date: 2024-03-29T19:38:45+10:00
draft: true
type: "post"
slug: "sql-performance-debugging-for-noobs"
tags: ["sql", "perf"]
---

Databases are magic black boxes that we developers just live with.  
Sure, we *could* understand them, but:  

![Ain't nobody got time for that!](http://www.quickmeme.com/img/94/94d74643939cd685a54ae8065ce91cd3f66a8aa727239585983bd879b07b2793.jpg)

So what can we mere lazy mortals do when SQL Server databases misbehave?  

<!--more-->  

> Disclaimer: I'm not an expert, take the below with a grain of salt and measure!  

# Check the queryplan  
For the luckily uninitiated, SQL Server parses your query and ~~makes a pact with Cthulhu~~ determines how best to execute it.  
The result is a query plan, which it can often reuse so it doesn't have to do it next time.

Each element of queryplans indicates a step the database takes and it keeps track of how long each step took and how much work it had to do.  

To investigate query plans using SSMS or Azure Data Studio:  

1. Enable getting the actual query plan (estimated query plan is a guess, and why when we can get the real one?)  
2. Run the query (ideally the exact query as it is executed in Prod from debug logs or whatever you use)  
3. Look through the plan for expensive operations

> Plans read right to left, bottom to top.  

Of course the last part is the second hardest, what is an expensive operation, and then the hardest is how to fix it?  

## What to look for  

- **Table scan:** The DB is looking through the whole table. That's slow.  
- **Index scan:** The DB is looking through an index. That's much faster, but still not as fast as it could be (Index seek).  
- **Parallelism:** Little orange icons with arrows in the query plan mean the DB can do data fetching and processing in paralell. This is usually faster as you can imagine, however many things can prevent paralellism (e.g. user defined functions)  
- **Excessive memory grant:** Little orange exlamations in the query plan indicate warnings. This one is usually where it got more rows than expected for one or more of the steps. This is usually a statistics problem.  
- **Missing statistics:** Related to the above, you can try running statistics or manually generating them.  
- **Some steps working with way more rows than expected:** These two are hard, but if the right side of the plan is processing millions of rows, but ultimately the left returns two rows, it might be worth trying to alter the query to limit the right side rows.  
- **Many more rows in the result-set than expected:** Similar to above, if the left is returning loads of near duplicate rows, it's worth reconsidering your query, possibly splitting it or filtering in other places.  

## Analyse query plan  
Right click anywhere on the plan and select "Analyse query plan".  
I've only been able to get it to show inaccurate cardinality estimates.  
If there are any there that are really high, it can indicate your statistics are out of date.  
These are out of my knowledge. I just run `exec sp_updatestats`, I suspect this isn't wise to run in Production though.  

# Check on missing indexes  
Indexes can make a massive difference to performance.  
They're essentially a smaller table built off the larger table specifically for performance.  

Sometimes when running the query in SSMS, you'll get a green box with "Missing Index:" and the `create index` method.  
It would be a terrible idea to copy paste that, give it a proper name, and run it on the database...  
So after you do that, performance should improve for that query, but sometimes not.  
Sometimes you won't get a suggested index, and sometimes you don't want to check every single query your app runs.  

The following query is one I ~~stole from several places~~ cooked up myself...  

```sql
SELECT TOP 20
    CONVERT (varchar(30), getdate(), 126) AS runtime,
    CONVERT (decimal (28, 1), 
        migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) 
        ) AS estimated_improvement,
    'CREATE INDEX missing_index_' + 
        CONVERT (varchar, mig.index_group_handle) + '_' + 
        CONVERT (varchar, mid.index_handle) + ' ON ' + 
        mid.statement + ' (' + ISNULL (mid.equality_columns, '') + 
        CASE
            WHEN mid.equality_columns IS NOT NULL
            AND mid.inequality_columns IS NOT NULL THEN ','
            ELSE ''
        END + ISNULL (mid.inequality_columns, '') + ')' + 
        ISNULL (' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement
FROM sys.dm_db_missing_index_groups mig
JOIN sys.dm_db_missing_index_group_stats migs ON 
    migs.group_handle = mig.index_group_handle
JOIN sys.dm_db_missing_index_details mid ON 
    mig.index_handle = mid.index_handle
ORDER BY estimated_improvement DESC;
GO
```

This query shows the top 20 recommended missing indexes recommended by Our Lord Cthulhu.  
There is an estimate there too, although I don't really know much about it.  

# Parameter sniffing  
My understanding is limited here, but when a query plan is built, it uses the statistics (which is why stale stats can be a problem) and the parameters that are going to be substituted into the plan.  
If it guesses wrong about what those parameters look like, it can work fine for some parameters and awfully for others.  
I can safely say I don't know how to fix this, except using `OPTION (RECOMPILE)` which builds a very general plan which tends to balance ok BUT will rebuild it each time, which can be terrible for performance.  

The only way I know how to identify this is when you have a poorly performing query in Prod, yet it performs very well in SSMS.  
e.g. Prod takes 50 seconds, SSMS takes 0.1 seconds (over similar data)  

# Compatibility level?  
SQL Server has pretty good backwards compatibility, and exposes it via compatibility levels.  
You can have SQL Server 2022 installed but make the database behave as if it was 2012, which is very useful for moving databases or upgrading infrastructure.  
There can be performance benefits to upping compatibility levels, in recent times around paralellism.  

It's a long shot, but as long as you're not using advanced SQL Server features, it's worth a go.  

# Summary  
Can databases be tamed? Can your sql performance woes be solved?  
No.  

Buuut you might be able to trick the Old Ones every now and then.  