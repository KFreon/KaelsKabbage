---
title: "Migrating Newtonsoft to System.Text.Json"
date: 2021-08-02T16:13:30+10:00
type: "post"
slug: "migrating-newtonsoft-to-system-text-json"
tags: ["net5", "json"]
---

[System.Text.Json](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/) was released with dotnetcore 3 in 2019, and has been improved in NET 5.  
After recently upgrading [this project]({{< ref "/posts/18-migrating-to-dotnetcore3-with-efcore/index.md" >}}) to dotnetcore 5 (which was much easier than the linked upgrade to dotnetcore 3...), I was curious to see what the migration path looked like for a real project.  

<!--more-->  

The [documentation](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0) spells out that System.Text.Json is not a direct replacement for [JSON.NET](https://www.newtonsoft.com/json) (formerly Newtonsoft.JSON) and is intended for standard, fairly simple serialisation requirements.  

I thought "Our serialisation is fairly standard, just NodaTime".  
Let's say I misjudged, but not necessarily because they're not standard.  

To elaborate, the main issues were due to some decisions we made. They weren't incorrect or bad decisions, but they weren't "simple" enough for these API's.  

> There are many differences between Newtonsoft and System.Text.Json, the issues below are all the issues I had, but you might come across others.  
Be sure to read [more](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#table-of-differences-between-newtonsoftjson-and-systemtextjson) in the docs.  

Let's take a look at some of the issues/hurdles I came across.  

{{< toc >}}

# The Setup  
- Aspnetcore 5 (recently upgraded!)  
- Autofac for dependency injection  
- JSON.NET (I'll refer to it as Newtonsoft hereon, it's what I'm used to)  
- NodaTime  
- EFCore 5 (not really relevant here but for context on some decisions made)  
- This was a SPIKE! Much of the changes I made could probably be done in better ways, but I often wanted to keep the style, etc.  

# Timespan isn't (really) supported   
The C# `TimeSpan` struct isn't properly supported in serialisation or deserialisation.  
If you try it out of the box, it's treated as any other object, resulting in all the fields being serialised. This isn't the format we'd expect for a `TimeSpan`.  
In this project, we expected `00:00:00` as this is the default for Newtonsoft. There is an ISO format for [durations](https://en.wikipedia.org/wiki/ISO_8601#Durations), which is being considered for the eventual implementation in System.Text.Json.  

It's a relatively simple converter, but I instead used [this Nuget package](https://www.nuget.org/packages/Macross.Json.Extensions/).  
The decision to not support TimeSpan baffles me, but it seems one thing they were concerned about was the format to serialise from/to. Hopefully this is supported in later NET versions.     

# JsonPropery Required Attribute  
Another sticking point was our use of `JsonProperty(Required)` on some of our attributes.  
The docs [call this out](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#required-properties) and provide a workaround, however I struggled a little with their examples, as it didn't really feel nice or practical to do it that way.  
Eventually, I settled on a method of generating custom converters for any types with `Required` properties at startup via generics and DI (Autofac in this case):    

{{< splitter >}}
{{% split side=left title="DI Registration" %}}
``` csharp
public static class JsonSerializerOptionsExtensions
{
  public static JsonSerializerOptions RegisterSystemJsonRequiredPropertyConverters(this JsonSerializerOptions options, Assembly assembly)
  {
    var typesWithRequiredProperties = assembly
      .GetTypes()
      .Select(x => new
      {
        Type = x,
        RequiredProperties = x
          .GetProperties()
          .Select(p => new
          {
            Prop = p,
            IsRequired = p.GetCustomAttribute<SystemJsonRequiredAttribute>()
          })
          .Where(a => a.IsRequired is not null)
      })
      .Where(x => x.RequiredProperties.Any())
      .ToArray();

    var genericRequiredPropertyConverterTypeDef = typeof(SystemJsonPropertyRequiredConverter<>).GetGenericTypeDefinition();

    var converters = typesWithRequiredProperties
      .Select(x => new {GenericTypeDef = genericRequiredPropertyConverterTypeDef.MakeGenericType(x.Type), RequiredPropNames = x.RequiredProperties.Select(p => p.Prop.Name) })
      .Select(x => Activator.CreateInstance(x.GenericTypeDef, x.RequiredPropNames.ToArray()));

    foreach(var converter in converters)
    {
      options.Converters.Add(converter as JsonConverter);
    }

    return options;
  }
}
```
{{% /split %}}
{{% split side=right title="Custom Converter" %}}
``` csharp
public class SystemJsonRequiredAttribute : Attribute
{

}

public class SystemJsonPropertyRequiredConverter<T> : JsonConverter<T> where T : class
{
  private readonly Func<T, object>[] _requiredPropertyAccessors;

  // We don't want to create this many times: https://www.meziantou.net/avoid-performance-issue-with-jsonserializer-by-reusing-the-same-instance-of-json.htm
  private readonly JsonSerializerOptions _privateOptions;

  public SystemJsonPropertyRequiredConverter(params string[] requiredPropertyNames)
  {
      _requiredPropertyAccessors = requiredPropertyNames.Select(x => CreatePropertyAccessor<T>(x)).ToArray();
      var privateOptions = new JsonSerializerOptions();
      privateOptions.ConfigureCommonJsonOptions();
      _privateOptions = privateOptions;
  }

  public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var obj = JsonSerializer.Deserialize<T>(ref reader, _privateOptions);
    foreach (var propertyAccessor in _requiredPropertyAccessors)
    {
      var val = propertyAccessor(obj);
      if (val == default)
      {
        throw new InvalidOperationException("Parameter must be set.");
      }
      else if (val is string str)
      {
        if (string.IsNullOrEmpty(str))
        {
          throw new InvalidOperationException("Parameter must be set.");
        }
      }
    }

    return obj;
  }

  public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
  {
    JsonSerializer.Serialize<T>(writer, value, _privateOptions);
  }

  // Fancy stuff from: https://blog.zhaytam.com/2020/11/17/expression-trees-property-getter/
  // Intended to be more performant, though I'd likely need to cache these instead of recreating them each time!
  private static Func<Y, object> CreatePropertyAccessor<Y>(string propertyName)
  {
    var parameterExpression = Expression.Parameter(typeof(object), "instance");
    var instanceExpression = Expression.TypeAs(parameterExpression, typeof(Y));
    var propertyExpression = Expression.Property(instanceExpression, propertyName);
    var propertyObjExpression = Expression.Convert(propertyExpression, typeof(object));
    return Expression.Lambda<Func<Y, object>>(propertyObjExpression, parameterExpression).Compile();
  }
}
```
{{% /split %}}
{{< /splitter >}}  

# JObject/JsonDocument Property Access differences  
In Newtonsoft, you can do:  

``` csharp
var obj = JsonSerializer.Deserialize<JObject>(json);
var intValue = obj["prop"][0].Value<int>();
```  

You can't do any of that in these new APIs.  
The [docs](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#how-to-search-a-jsondocument-and-jsonelement-for-sub-elements) indicate that it wasn't designed for this kind of thing. You're not supposed to inspect arbitrary json.  However...many of our tests just grab the data and look at it.  
Since this was a spike for experimentation purposes, I wanted to try to keep this style.  
Several extension methods later, I got this:  

``` csharp
var obj = JsonSerializer.Deserialize<JsonDocument>(json);
var intValue = obj.GetAsObject()["prop"].GetAsArray()[0].GetInt32();
```

The extension methods:  

``` csharp
public static class JsonDocumentExtensions
{
  public static string GetPropertyValue(this JsonDocument document, string name)
  {
    return document.RootElement.GetPropertyValue(name);
  }
}

public static class JsonElementExtensions
{
  public static string GetPropertyValue(this JsonElement element, string name)
  {
    return element.EnumerateObject().First(x => x.Name == name).Value.GetString();
  }

  public static int? GetNullableInt(this JsonElement element)
  {
    if (element.ValueKind == JsonValueKind.Null)
    {
      return null;
    }

    return element.GetInt32();
  }

  public static Dictionary<string, JsonElement> GetAsObject(this JsonElement element)
  {
    if (element.ValueKind == JsonValueKind.Object)
    {
      return element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
    }
    throw new InvalidOperationException("Not an object");
  }

  public static JsonElement[] GetAsArray(this JsonElement element)
  {
    if (element.ValueKind == JsonValueKind.Array)
    {
      return element.EnumerateArray().ToArray();
    }
    throw new InvalidOperationException("Not an array");
  }
}
```  

# Constructor Restrictions    
There are a fair few restrictions on constructors and their behaviours, especially coming from Newtonsoft, which didn't really care at all.  

## Parameters and properties must match (name and type)  
We use a lot of `IReadOnlyCollections` in our DTO's, and they're usually simple DTOs, like:  

```csharp
public class SomeDto 
{
  public IReadOnlyCollection<string> SomeProp { get; }

  public SomeDto(IEnumerable<string> things) {
    SomeProp = things.ToImmutableArray();
  }
}
```

The above is fine in Newtonsoft, but not allowed in System.Text.Json for several reasons.  

- Parameter types must match property types  
  - In the above example, it doesn't like that the types mismatch between `IReadOnlyCollection` and `IEnumerable`. They must be the same type.  
- Parameter names must match property names
  - `SomeProp` is not the same as `things` and will cause a runtime exception.  

Both of the above give exceptions that look similar to: `Each parameter in constructor must bind to an object property or field on deserialization. Each parameter name must match with a property or field on the object. The match can be case-insensitive.`  

This was BY FAR, the biggest cause of pain in this migration. We had so many DTO's with the pattern demonstrated above, and some of the name differences were really hard to spot.  
e.g. `jobId` vs `id` when it's the fifth parameter in a 20 parameter constructor, painful.  
Which actually leads me to...

## Constructors cannot have more than 64 parameters   
There was only one instance in the codebase was easily the biggest, most detailed object we had, and it had 85 parameters.  
It's a runtime exception indicating that the limit is 64 parameters, and this appears to be a hard, unchangable limit.  
I refactored it to group some of the parameters into sub-objects.   

## Multiple constructors is kinda supported  
Technically, multiple constructors is supported, but the behaviour is a bit different than Newtonsoft.  
I'm actually not sure whether Newtonsoft chooses the best-fit constructor or uses private setters (if allowed), but the result is that our standard Dtos (as above) failed when we have multiple constructors.  
Usually, there were multiple constructors because we had a parameterless constructor (usually for EF use), and a parameterised constructor for normal use.  
Unfortunately, the parameterless constructor is the default choice for System.Text.Json when it exists.  

This resulted in none of the properties being set, as the setters were private and System.Text.Json can't set private properties without help (see below).   
The workaround is to remove multiple constructors (where possible), or to use the `[JsonConstructor]` attribute to specify which constructor you want it to use.  
> Parameterless constructors didn't cause runtime exceptions, but created a default object with no properties set.  
> Multiple parameterised constructors DID cause runtime exceptions.  

I should point out that all of the above issues were explained quite well in their respective exception messages, so while it was frustrating that these things didn't work, I was being told exactly what was wrong and sometimes why.  

# Base classes and Private setters  
In this project, we have `AggregateRoot`s to help manage the database. They tend to look like this:  

```csharp  
public abstract class AggregateRoot 
{
  public string CreatedBy { get; private set; }
  public DateTimeOffset CreatedOn { get; private set; }

  public void RecordCreation(string createdBy, DateTimeOffset createdOn)
  {
    CreatedBy = createdBy;
    CreatedOn = createdOn;
  }
}

public class SomeAggregate : AggregateRoot
{
  public int CountOfMontyChristo { get; private set; }

  public SomeAggregate(int countOfMontyChristo)
  {
    CountOfMontyChristo = countOfMontyChristo;
  }
}

// Then used later like:
var agg = new SomeAggregate(4);

/* In DB saveChanges */
agg.RecordCreation("me", DateTimeOffset.Now);
```  

In the above, deserialisation wouldn't set `CreatedBy` or `CreatedOn` because it doesn't have access to the setters.  
You'd end up with:   

``` csharp
CountOfMontyChristo = 4;
CreatedBy = null;
CreatedOn = 01/01/0001 00:00:00;
```  

In order to get those parameters set, you can put a `[JsonInclude]` attribute on the properties.  
Another option is to add a base constructor and use that, which would require `[JsonConstructor]` attribute as well.  

{{< splitter >}}
{{% split side=left title="JsonInclude Example" %}}
```csharp  
public abstract class AggregateRoot 
{
  [JsonInclude]
  public string CreatedBy { get; private set; }

  [JsonInclude]
  public DateTimeOffset CreatedOn { get; private set; }

  public void RecordCreation(string createdBy, DateTimeOffset createdOn)
  {
    CreatedBy = createdBy;
    CreatedOn = createdOn;
  }
}
```
{{% /split %}}
{{% split side=right title="JsonConstructor Example" %}}
``` csharp  
public abstract class AggregateRoot 
{
  public string CreatedBy { get; private set; }
  public DateTimeOffset CreatedOn { get; private set; }

  public void RecordCreation(string createdBy, DateTimeOffset createdOn)
  {
    CreatedBy = createdBy;
    CreatedOn = createdOn;
  }

  // Add base constructor
  public AggregateRoot(string createdBy, DateTimeOffset createdOn)
  {
    CreatedBy = createdBy;
    CreatedOn = createdOn;
  }
}

public class SomeAggregate : AggregateRoot
{
  public int CountOfMontyChristo { get; private set; }

  public SomeAggregate(int countOfMontyChristo)
  {
    CountOfMontyChristo = countOfMontyChristo;
  }

  // Add this constructor for serialisation.
  [JsonConstructor]
  public SomeAggregate(int countOfMontyChristo, string createdBy, DateTimeOffset createdOn) : base(createdBy, createdOn)
  {
    CountOfMontyChristo = countOfMontyChristo;
  }
}
```
{{% /split %}}
{{< /splitter >}}  

# Other Little Things      
- [NodaTime](https://nodatime.org/) required a System.Text.Json version of their serialisation helper [Nuget package](https://www.nuget.org/packages/NodaTime.Serialization.SystemTextJson).  
- Reuse `JsonSerializerOptions` via DI as there's some [perf considerations](https://www.meziantou.net/avoid-performance-issue-with-jsonserializer-by-reusing-the-same-instance-of-json.htm) there.  

# Summary/TL;DR  

- `TimeSpan` requires some work to support.  
- `JsonProperty(Required)` is not supported and needs a custom converter workaround.  
- `JsonDocument` is not as easy to navigate as `JObject` and requires verbosity, or extension methods.  
- Constructor parameters must match the properties' name and type.  
- Constructors cannot have more than 64 parameters (I know, it's a lot, but we had it).  
- Multiple constructors require `[JsonConstructor]` attribute.
- Private setters required `[JsonInclude]` to be deserialised to.  

Ultimately, the default options were generally suitable, with some fairly simple workarounds.  
However, the work required to adjust ALL those DTO's constructors was far more work than I expected, and finding them was also a great pain.  
