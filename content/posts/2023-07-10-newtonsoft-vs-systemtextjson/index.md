---
title: "Newtonsoft vs System.Text.Json: Memory Allocations using BenchmarkDotNet"
date: 2023-07-10T16:13:25+10:00
type: "post"
slug: "newtonsoft-vs-system-text-json"
tags: ["json"]
---

Recently, I've been investigating some poor memory behaviour in an Azure App Service and had the usual niggle to dig into performance again.  
This time, it's about serialisation performance, specifically regarding memory allocations.  

<!--more-->  

# Why am I looking at Serialisation?   
The app in question is performing badly, but there's nothing obvious in App Insights.  
is running alongside a load of others and the tier is just too low, and Azure calls out one or two of the apps for "high memory" at around 300mb...    

We should/will upgrade it's tier, but first I wanted to see if there was something simple I could do.  
That escalated into investigating replacing [Newtonsoft](https://www.newtonsoft.com/json) with [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-7-0)

There are a number of comparisons between these two online, but they often used toy examples and I wanted to know what a real Enterprise example would look like.  
Proper Enterprise, you know, megabytes of json.  

## Profiling  
Before I go further, yes I did some profiling, and no it didn't really point at serialisation as an issue.  
It did show huge allocations and GC time waiting and I had a thought that it could be the massive objects we're serialising out to the UI.  
I thought "Even if it isn't that (since it wasn't really showing in my profiler) I want to see what the difference is.  
> The profilers call out EFCore as the highest allocator for reading data from DB, but I can't really do anything about that...  

# Setup  
I grabbed an example output from my Test environment, and it looks like this:  
- Minified = 3.8mb  
- Formattted = 162379 lines  
- Lots of nesting and a mix of data types  

> This is the main page payload, so every user gets this every time they load the site, so I think it's a reasonable target for investigation.  

Also here's the output from [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) regarding my machine.  

```  
BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.23493.1000)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=8.0.100-preview.5.23303.2
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2  
```

## Caveats  
There's loads of weird things in this app, and I had to do some work to get this to be possible.  
As such, I've only really done enough for it to **be** possible, not to make it good and proper.  
Doubtless there's many things I could do to improve this on both sides.  

Also, the changes I had to make to get it to work weren't a 5 minute job.  
There's a bunch of differences between these two implementations, see my older post [Migrating Newtonsoft to System.Text.Json]({{< ref "/posts/28-newtonsoft-to-system-net-json/index.md">}}).  

# Benchmarks  
Here's my code for the benchmarks  

{{< splitter >}}
{{% split side=left title="Serialisation" %}}
```cs
[MemoryDiagnoser]
public class JsonSerialisationTests
{
    private readonly JsonSerializerOptions options = new JsonSerializerOptions()
    {
        // This is important, it apparently slows things down a bit.
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            // ...a bunch of converters, mostly for enums
        }
    };
    private string jsonString = HugeJsonString.JsonString;
    private DTO obj = null;

    private JsonSerializerSettings newtonsoftOpts = new JsonSerializerSettings()
    {
        Converters =
        {
            //... converters
        }
    };

    [GlobalSetup]
    public void Setup()
    {
        obj = JsonConvert.DeserializeObject<DTO>(jsonString, newtonsoftOpts);
    }

    [Benchmark]
    public string SystemTextJsonSerialise()
    {
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }

    [Benchmark(Baseline = true)]
    public string NewtonsoftSerialise()
    {
        return JsonConvert.SerializeObject(obj);
    }
}
```
{{% /split %}}
{{% split side=right title="Deserialisation" %}}
```cs
[MemoryDiagnoser]
public class JsonDeserialisationTests
{
    private readonly JsonSerializerOptions options = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            //...converters
        }
    };
    private string jsonString = HugeJsonString.JsonString;
    private DTO obj = null;

    private JsonSerializerSettings newtonsoftOpts = new JsonSerializerSettings()
    {
        Converters =
        {
            // ...converters
        }
    };

    [GlobalSetup]
    public void Setup()
    {
        obj = JsonConvert.DeserializeObject<DTO>(jsonString, newtonsoftOpts);
    }
    
    [Benchmark]
    public DTO SystemTextJsonDeserialise()
    {
        return System.Text.Json.JsonSerializer.Deserialize<DTO>(jsonString, options);
    }

    [Benchmark(Baseline = true)]
    public DTO NewtonsoftDeserialise(JsonSerializerSettings newtonsoftOpts)
    {
        return JsonConvert.DeserializeObject<DTO>(jsonString, newtonsoftOpts);
    }
}
```
{{% /split %}}
{{< /splitter >}}  

# Results  
And drumroll 🥁🥁🥁  
## Serialisation Benchmark  
|                  Method |     Mean |    Error |   StdDev | Ratio |      Gen0 |     Gen1 |     Gen2 | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|------:|----------:|---------:|---------:|----------:|------------:|
| SystemTextJsonSerialise | 10.84 ms | 0.064 ms | 0.054 ms |  0.45 |  265.6250 | 265.6250 | 265.6250 |   7.44 MB |        0.43 |
|     NewtonsoftSerialise | 24.05 ms | 0.433 ms | 0.384 ms |  1.00 | 1062.5000 | 968.7500 | 250.0000 |  17.24 MB |        1.00 |

## Deserialisation Benchmark  
|                    Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen0 |      Gen1 |     Gen2 | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|---------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| SystemTextJsonDeserialise | 47.82 ms | 0.942 ms | 1.548 ms |  0.58 |    0.03 | 1083.3333 |  750.0000 | 250.0000 |  17.82 MB |        0.61 |
|     NewtonsoftDeserialise | 82.80 ms | 1.655 ms | 3.067 ms |  1.00 |    0.00 | 2714.2857 | 1142.8571 | 285.7143 |  29.26 MB |        1.00 |

# Conclusions  
The benchmarks are pretty clear: System.Text.Json is faster and more memory efficient.  
This does come with the caveat that it's also a needy little thing as well, and I had a lot of converters and reworking constructors as per my other post on [Migrating Newtonsoft to System.Text.Json]({{< ref "/posts/28-newtonsoft-to-system-net-json/index.md">}})  

I won't be rushing out to move all the things away from Newtonsoft, but I won't be reaching for it either.  