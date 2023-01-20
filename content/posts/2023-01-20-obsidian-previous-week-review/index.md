---
title: "Obsidian: Previous Week Review"
date: 2023-01-20T13:12:14+10:00
type: "post"
slug: "obsidian-previous-week-review"
tags: ["obsidian"]
---

I've been playing with [Obsidian](https://obsidian.md/) over the last few months, and aside from some minor UI changes (via CSS) it's been pretty good!  
However, I found that I wanted to see what I was up to last week in the context of what I'm doing today.  
Here's how I did it.

<!--more-->  

My primary use of Obsidian is taking notes through the week. In general, I do work for different customers each day of the week, and I find that I've forgotten what I was doing last week, OR I did some work for that client on a different day than normal.  
I thought it'd be nice to have some way of summarising the last week of work to remind me.  

# Dataview  
Obsidian has a plugin called [Dataview](https://github.com/blacksmithgu/obsidian-dataview) which exposes a Javascript API and allows querying and advanced data visualisations over your notes.  
You can do basic stuff of the SQL vein like `table file from "incident"` which shows all your notes where you had an `#incident` tag.  
There is also a much more advanced Javascript implementation allowing you to add arbitrary code to get what you want.  

# Summary View V1
My first go at the summary view uses the SQL approach.
Essentially, I ask it to get me a list of the files from the last 7 days which have occurrences of the tags in the current page in them.  

e.g. The current page has `#incident` from above, so my summary would show any other files in the last week that have that tag.  

```sql
--```dataview
table file.tags as Tags 
where file.ctime > (this.file.ctime - dur(1 week)) and (file.ctime < this.file.ctime) and any(file.tags, (x) => contains(this.file.tags, x))
--```
```

However, you might notice an issue: It only gets 7 days back. For Monday, this is fine as it'll get the whole week prior, but for Friday, it'll only get back to Friday...
That's not quite what I want.

# Summary View V2
I had to delve into JS to achieve what I wanted here, as I couldn't find another way of extracting and comparing the dates with the start of their weeks, etc

```js
//```dataviewjs
let fileDate = dv.date(dv.current().file.name.split(" ")[0])
let startOfWeek = fileDate?.startOf('week')
let fileTags = dv.current().file.tags

let mySource = dv.pages().filter(x => {
  const d = dv.date(x.file.name.split(" ")[0])
  if (!d || x.file.name === dv.current().file.name) return false
  const dStartOfWeek =  d.startOf('week')
  const diff = startOfWeek - dStartOfWeek
  const withinDateRange = diff >= 0 && diff <= dv.duration('7 days')
  const hasMatchingTags = x.file.tags.some(t => fileTags.includes(t))
  return withinDateRange && hasMatchingTags
}).map(x => [x.file.link, x.file.tags])

dv.table(['File', 'Tags'], mySource)
//```
```

# The Result
{{< image path="img/ObsidianDataviewV2" alt="Looks pretty shiny to me!" >}}