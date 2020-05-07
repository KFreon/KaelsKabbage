---
title: "C# - Static members in Generic classes are weird"
date: 2020-03-04T14:51:26+10:00
draft: false
type: "post"
slug: "csharp-statics-in-generics"
tags: ["c sharp", "generics"]
---

[Generic classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/) are useful coding constructs that I thought I mostly understood, but I had an issue with them recently that blew my mind... Static members in Generic classes don't work as I naively expected.   

<!--more-->  

# Setup
The conversation went something like this:  

- The tests fail due to some concurrency issues in the database prework for test setup.   
- It has async code, so add `private static readonly SemaphoreSlim` and we're done!   
- Wait...that didn't help?   
- Debugging shows it's still concurrent?! HOW!?!?!11  

{{< image path="img/Statics-Generics" alt="Parallel stacks indicating the bypassing of the semaphore" >}}  

# Background  
I had: 

- Abstract base class to perform test setup  
- Several concrete implementations for various test scenarios  
- Recently (and slipping my mind) made generic for readbility

# How is it so?
I jumped onto my internal company channels for assistance (after assuring myself that `static` really did what I thought it did).  
The conversation wound around as I re-explained my poorly worded initial question, but the themes were:  

- Are you really sure all those threads are PAST the semaphore, or just one and the rest are waiting?  
- Maybe the debugger isn't behaving as expected?   
- Maybe the test suite isn't behaving as expected?   

As I was investigating these avenues, someone piped up with: 

> "Statics in generic classes are not the same value in each of their concrete implementations."  

This immediately drew my attention since it was a bit odd. Surely statics are...**static**? That's the point?  

The conversation bounced around on this topic a bit, and it does turn out that statics in generic classes don't behave as you might initially expect.  
Static members in generic classes aren't the same as in normal classes (on the face of it)  

# Explanation  
Generic classes take type parameters, and the static member you have is static, but only within that generated concrete type.  
e.g.  
``` cs
public class GenericTest<T> {
  public static int ImStatic;
}

var str = new GenericTest<string>();
var number = new GenericTest<int>();

str.ImStatic != number.ImStatic;  // Brainsplosion!
```  
  
It kinda makes sense when I think of Generics as templates, and the compiler generates a concrete implementation of them for each type used in your code, then each type parameter is a different class.  
I'm aware that's not quite how it works, and I don't really know how it works, but this works in my head :)  

The IDE's have [warnings](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1000?view=vs-2019) about this as well, although the default for VS was a bit lax, barely noticable.  

# Solution  
The solution we went with was another static class, unrelated to the others, that just held the Semaphore for locking purposes.  
Not the nicest or cleanest, but it's not complicated and does the job.  

``` cs
internal static class TestDBSetupLocker {
  private static SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

  public static Task Lock() {
    return _locker.WaitAsync();
  }

  public static void Release() {
    _locker.Release();
  }
}

public abstract class TestBase<T> {
  public async Task Setup() {
    await TestDBSetupLocker.Lock();

    // ... DB setup stuff

    TestDBSetupLocker.Release();
  }
}
```