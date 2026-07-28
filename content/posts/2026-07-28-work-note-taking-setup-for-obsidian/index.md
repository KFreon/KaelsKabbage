---
title: "Work note taking setup for Obsidian"
date: 2026-07-28T14:19:57+10:00
draft: true
type: "post"
slug: "work-note-taking-setup-for-obsidian"
tags: ["obsidian"]
---

I watched a few videos on how to do good notetaking with Obsidian, but they all involved processes, and strategies, and revisiting and whatnot, and I knew I wasn't going to do that, or at least that I would forget.
I wanted the easiest possible entry, which for me was just starting to write notes.
Over time, I added and augmented more and more until I developed my own style, and I suggest you do so as well.

This post is about what works for me, but it could spark ideas for you or prompt you to setup something similar.
I'm not going to work through from the beginning, but explain what I have now and why.

<!--more-->  

# What is Obsidian  

For those unaware, [Obsidian](https://obsidian.md/) is a note taking app that stores data as markdown files.  
It's similar to [Notion](https://www.notion.com/) but not as fancy looking (maybe you could say Notion is the Apple and Obsidian is the Android) 

I used to use Onenote, but then I had to move my notes from one account to another and that SUCKED so bad, so I looked to switch.  
The main downside that I wasn't really able to get was raw drawings to markup my notes (I know about the [Excalidraw]() plugin, but it's a bit too fiddly for me).
With that caveat aside though, Obsidian does pretty much everything else I want, in the way I want.  

# Obsidian Setup 
There are lots of core and community plugins to play with.  
I have various ones enabled and installed, but I'd say the critical ones are:

## Core  
- Properties View
- Daily Notes (not actually sure how much this is used, I don't have the `template file location` set, just the `new file location`)

## Community  
- Advanced tables (makes working with markdown tables way easier)
- Calendar (to get that nice calendar view to click on)
- Dataview
- Natural Language Dates
- Date format: `[Work Tracking]/gggg-MM-DD`. 
    - `gggg` is the week year, that is, when a week spans a year, the days falling in the next year, it'll still be in the year from the start of the week...maybe I need to fix that. I don't remember why I did it this way.  
- Periodic notes (for templating the daily notes)

![Periodic notes setup](img/DailyNotes_Setup.png)

> Note I'm using two templates, one for daily and one for weekly. I'll come back to that.  

That's a few more than I thought actually!  

# Usage

My primary usecase is [daily notes](https://obsidian.md/help/plugins/daily-notes) where I can click on the date in a calendar and it creates the note for that day.
These notes can be customised with templates, and I like to have a [previous week review](/content/posts/2023-01-20-obsidian-previous-week-review/index.md) which gets the uncompleted tasks from the last few weeks (naming is hard alright...)

## Daily note template  
This is a crazy bit of madness that I scrapped together, and I swear the JS is terrible, but I had a hard time with the documentation and manual digging into the API. 

{{% splitter %}}
{{% split side=left title="Daily notes Example" %}}
![Daily Note example](img/DailyNotes_Example.png)
{{% /split %}}
{{% split side=right title="Another example" %}}
![Another example](img/DailyNotes_AnotherExample.png)
{{% /split %}}
{{% /splitter %}}  


````c
---
creationDay: "{{date:dddd}}"
---
# Previous week review
```dataviewjs
// Dev console: Ctrl + shift + i

let currentFile = dv.current().file
let currentYear = currentFile.day.year
let currentWeekNumber = currentFile.day.weekNumber

let matchingFiles = dv.pages().filter(x => {
  const d = x.file.day
  if (!d || x.file.name === currentFile.name) return false

  // This week or last week
  // Note this doesn't work over the new year, but that's ok
  const isInThisYear = currentYear === d.year
  const isInThisWeek = currentWeekNumber === d.weekNumber
  const isInLastWeek = d.weekNumber === (currentWeekNumber - 2)
  const notInTheFuture = currentFile.day.ts >= d.ts
  const withinDateRange = notInTheFuture && isInThisYear && (currentWeekNumber - d.weekNumber) <= 2
  return withinDateRange
})

const tasks = matchingFiles.flatMap(f => f.file.tasks.where(t => !t.completed))

dv.taskList(tasks)
```

```dataviewjs
const currentFile = dv.current().file
let luxonDate = currentFile.day
let currentYear = luxonDate.year
let currentWeekNumber = luxonDate.weekNumber
let fileTags = currentFile.tags

let matchingFiles = dv.pages().filter(x => {
  const d = x.file.day
  if (!d || x.file.name === currentFile.name) return false

  // This week or last week
  // This doesn't work over the new year, but that's ok
  const isInThisYear = currentYear === d.year
  const isInThisWeek = currentWeekNumber === d.weekNumber
  const isInLastWeek = d.weekNumber === (currentWeekNumber - 1)
  const withinDateRange = isInThisYear && (isInThisWeek || isInLastWeek)
  
  const hasMatchingTags = x.file.tags.some(t => fileTags.includes(t))
  return withinDateRange && hasMatchingTags
})

const flattened = matchingFiles.flatMap(x => x.file.tags.filter(t => fileTags.includes(t)).map(t => ({tag: t, link: {...x.file.link, subpath: t}})))
const grouped = flattened.groupBy(x => x.tag)
const indexed = grouped.map(x => {
	const sorted = x.rows.sort(t => t.link.path)
	const cleanLinks = sorted.map(t => ({ subpath: t.link.subpath, path: t.link.path.replace('Work Tracking/', '').replace('.md', '') }))
	const strings = cleanLinks.map(t => `[[${t.path}${t.subpath}]]`)
	return [x.key, strings]
})

dv.table(['Tag', 'Files'], indexed)
```
---
````

I then want to be able to track what client I'm working on and generally for how long.
For that, I use tags and make them a header for linking purposes.
Then I can create a property of the hours spent on it that day.

> Ideally, I'd figure out how to automatically add a property when creating a heading + tag, but I haven't done that yet.  

You can see this hours tracking in the above screenshot, and they get pulled through into the Weekly note.  

## Weekly note for timesheeting  
I use the weekly note + client tags + properties hours tracking to essentially create a timesheet in one click.  
It's a decent summary of what was happening for that week, and it auto updating when you need to go back and tweak things (like forgetting to fill in hours and such)

![Weekly note example](img/WeeklyNote_example.png)

````c

```dataviewjs
const currentFile = dv.current().file
let luxonDate = DateTime.fromFormat(currentFile.name, "kkkk-'W'W")
let currentYear = luxonDate.year
let currentWeekNumber = luxonDate.weekNumber

let matchingFile = dv.pages().filter(x => {
  const d = x.file.day
  if (!d || x.file.name === currentFile.name) return false

  // This week or last week
  // This doesn't work over the new year, but that's ok
  const isInThisYear = currentYear === d.year
  const isInThisWeek = currentWeekNumber === d.weekNumber
  const isInLastWeek = d.weekNumber === (currentWeekNumber - 1)
  const withinDateRange = isInThisYear && (isInThisWeek)
  
  return withinDateRange
})

let indexed = matchingFile.flatMap(x => x.file.tags
	.map(t => ({ 
		tag: t, 
		link: {...x.file.link, subpath: t}, 
		weekday: x.file.day.weekdayShort, 
		toplevel: x 
	})))
.groupBy(x => ({tag: x.tag}))
.map(x => {
	let dayMap = {
	Mon: undefined,
	Tue: undefined,
	Wed: undefined,
	Thu: undefined,
	Fri: undefined,
	Sat: undefined,
	Sun: undefined
	}
	
	const sorted = x.rows.sort(t => t.link.path)
	const cleanLinks = sorted.map(t => ({ 
		toplevel: t.toplevel,
		weekday: t.weekday,
		subpath: t.link.subpath, 
		path: t.link.path.replace('Work Tracking/', '').replace('.md', ''),
		hours: t.toplevel[x.key.tag.replace('#','')] ?? 0
	}))
console.log(cleanLinks)
	
	cleanLinks.forEach(t => {
		const val = `[[${t.path}${t.subpath}|${t.hours}h]]`
		dayMap[t.weekday] = (dayMap[t.weekday] ?? '') + ' ' + val
	})
	
	const values = Object.values(dayMap)
	return [x.key.tag, ...values]
})

dv.table(['Tag', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'], indexed)
```
````

# Long running/multiday notes  
It's common for me to have an issue or major piece of work that I want to write ntoes on, like P1 tickets or long-running investigations.  
For that, I'd create a new note and link to it from each of the daily notes.
That way, it's all together running and reading down in one place.
Sometimes, I'll create headings of the current date in those big notes, so I can link to the specific day heading in the specific day.
You can also do `![[]]` wikilinks which shows the linked content, not just a hyperlink.

These big notes can get unweildy, and sometimes I create tasks `[]` within them, and they can get hard to track, so then at the top, I put a dataview block that collects all the uncompleted tasks from the note in one place.

![Multiday note example](img/MultidayNote_Example.png)