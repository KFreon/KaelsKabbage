---
title: "C# - Static members in Generic classes don't work"
date: 2020-03-04T14:51:26+10:00
draft: true
type: "post"
slug: "csharp-statics-in-generics"
tags: []
---

[Generic classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/) are useful coding constructs that I thought I mostly understood, but I had an issue with them recently that blew my mind... Static members in Generic classes don't work as you naively expect.   

<!--more-->  

The conversation went something like this:  
- The tests fail due to some concurrency issues in the database prework for test setup.  
- It has async code, so add `private static readonly SemaphoreSlim` and we're done!  
- Wait...that didn't help?  
- Debugging shows it's still concurrent?! HOW!?!?!11  

{{< image path="img/Statics-Generics" alt="Parallel stacks indicating the bypassing of the semaphore" >}}  

Before I continue, I had an abstract generic base class deciding the majority of the test setup logic, with some concrete implementations setting up the specific scenarios.  

I then jumped onto my internal company channels for assistance (after assuring myself that `static` really did what I thought it did).  
The conversation wound around as I re-explained my poorly worded initial question, but the themes were:  
- Are you really sure all those threads are PAST the semaphore, or just one and the rest are waiting? 
- Maybe the debugger isn't behaving as expected?  
- Maybe the test suite isn't behaving as expected?  

Eventually, someone piped up with: "Statics in generic classes are not the same value in each of their concrete implementations." which immediately drew attention since it was a bit odd. 
"Surely statics are...static? That's the point?"  

The conversation bounced around on this topic a bit as well, and it does turn out that statics in generic classes don't behave as you might initially expect.  
This was mindblowing to me, so maybe someone else will find it interesting.  

Generic classes take type parameters, and the static member you have is static, but only within that generated concrete type.  
e.g.  
``` go
public class GenericTest<T> {
  private static readonly int _imStatic;
}
```  

If you instantiate this class with `new GenericTest<string>`, all instances of this class with the type parameter `string` will share that static member, but if you have another instantiation `new GenericTest<char>`, it will have a different version of that static.  
It kinda makes sense if you think of Generics as templates, and the compiler generates a concrete implementation of them for each type used in your code, then each type parameter is a different class.  
I'm fully aware that's not quite how it works, and I don't really know how it works, but this works in my head :)  

The IDE's have warnings about this as well, although the default for VS was a bit lax, barely noticable.  

So the takeaway: Static members in generic classes aren't necessarily behaving the way you expect.