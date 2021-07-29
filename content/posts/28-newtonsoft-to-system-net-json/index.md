---
title: "28 Newtonsoft to System Net Json"
date: 2021-07-19T07:52:30+10:00
draft: true
type: "post"
slug: "28-newtonsoft-to-system-net-json"
tags: ["net5"]
---



<!--more-->  

***Newtonsoft --> JSON.NET***
JsonProperty(Required) ==== https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#required-properties
-- Many crazy things required to make this work

Some property access issues.
JObject can do obj["prop"]["deeperProp"].Value<string>()
let's just say we can't do that anymore.
After some extension methods, I can do:
obj.GetAsObject()["prop"].GetAsObject()["deeperProp"].GetString()


Oh dear, `IReadOnlyCollection` isn't supported? :'(
Seemingly, but the docs say it is...
Ok it is, BUT it needs to be non-null.
I had `private readonly IReadOnlyCollectio<thing> {get;}` and I got:
'Each parameter in constructor must bind to an object property or field on deserialization. Each parameter name must match with a property or field on the object. The match can be case-insensitive.'
Once I set that `collection` to an empty array `{get;} = Array.Empty<thing>();` it started working...
Ok it doesn't really. Looks like the main cause here is that I set stuff to Array.Empty, but it's an IreadonlyCollection, not an array.  
If I make the property a List<strign>, then pass an array<string> which the constructor `.tolist`s, it fails. 
POLYMORPHIC serialisation?

RIGHT SO
-  The issue is that the constructor type DOESN'T match the property type. 
THIS DOESN"T WORK
```
class Thing {
  public List<string> Items{get;}

  public Thing(IEnumerable<string> items) {
    Items = items.ToList();
  }
}
```

BUT THIS DOES
```
class Thing {
  public List<string> Items{get;}

  public Thing(List<string> items) {
    Items = items.ToList();
  }
}
```

Similar to above, names of constructor items must match their properties. 

lol we have a method with 84 construtor parameters, and the max is 64. 
I guess we need to refactor.

Timespan serialisation isn't supported for some reason, very weird.
There's a lot of tickets about it, and it's hopefully going to be in NET 6, but for now, I've used [Macross Extensions](https://blog.macrosssoftware.com/index.php/2020/02/16/system-text-json-timespan-serialization/)

More constructor things, it won't pick the 'best' one, and it can't use private setters, so if you don't have public setters and a parameterless AND a parameterised constructor, it'll use the parameterless and just ignore the properties (since it can't set them)
---- TEST THIS PROPERLY
Can use the [JsonConstructor] attribute to designate.


/././.././././
MOVE THE RENDER prev/next buttons, they're in the way.