---
title: "Casting NaN to Numerics"
date: 2019-10-25T09:50:37+10:00
draft: false
type: "post"
slug: "c-sharp-nan-numeric-casting"
tags: ["c#"]
---

I came across an interesting problem today where a cast from `double` to `int` resulted in `int.MinValue`.  
Further digging revealed that it was a `NaN` problem.  

How did I get a `NaN` in simple C#? Damn divide by zero, that's how.  
But wait, how did I not get an exception?  

I had:  

```c#
double first = 0;
double second = 0;
var result = (int)Math.Round(first / second);
```

In the above case, `result` was `-2147483648`, but why! Feels like an overflow, so expanding it out, I got:  

```c#
double first = 0;
double second = 0;
var division = first / second;       // NaN
var rounded = Math.Round(division);  // NaN
var result = (int)rounded;           // -2147483648
```

There it is! Divide by zero gives `NaN` and casting that to `int` gives the minimum.  
Seems strange...The [documentation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions#explicit-numeric-conversions) clarifies it for me though.

> When you convert a double or float value to an integral type, this value is rounded towards zero to the nearest integral value. If the resulting integral value is outside the range of the destination type, the result depends on the overflow checking context. In a checked context, an OverflowException is thrown, **while in an unchecked context, the result is an unspecified value of the destination type**.

Basically, by default I was in an `unchecked` context, which means variables don't get checked for overflow. Numeric casting in `unchecked` contexts always succeeds, with the result being an unspecified value. Could have been anything, could have been that when this code was written, the writer was getting the 0 they wanted.  
The fix I went for was to check whether the result was `NaN` and if so, just return 0 instead.  

I shared this with my colleagues, and we discovered that `0/0` is different to `n/0`, which is infinity and also casts to `-2147483648`.

Surprising that C# lets us fall into that trap!
