---
title: "Software Testing"
date: 2018-08-23T09:06:59+10:00
draft: false
type: "post"
---

Testing is an oft argued topic, as I discovered when I started at [Readify](https://readify.net/).  
As a bit of a testing noob, I'd only read a bit about testing and thought "That sounds useful, more things caught before deployment", but upon trying to write them, I found it hard to come up with sensible scenarios. Many of the tutorials I'd seen were around calculations which just aren't a part of my job in any way. 

This was likely due to my lack of knowledge around programming at the time (if not still) and lack of imagination. Reading through some of the tests in my projects I often pondered the point of them. I'm always adjusting them to fit the code, but not in a useful way. They didn't so much test scenario as tell me when I changed a scenario, which isn't necessarily a bad thing, but wasn't the point of that test.  

My colleague [Andrew Best](https://www.andrew-best.com/) linked me an [NDC talk](https://vimeo.com/189830215) about testing which was quite informative.   
The big takeaway I got from it was that tests should be:  

- Simple and short  
- Non-trivial and non-fragile  
- Deleting tests is OK  

These stuck with me because I’d been reading through the tests on one project which was testing a mapper. At one point, perhaps these tests were useful (the mapper did more than basic mapping in the past), but now it was just testing that values got assigned to variables.  

Chatting with workmates on the subject, they pointed out that this could still add value by picking up when a mapping was deleted or something but agreed that it was a fairly low value test and should be tested at a higher level.  

# Points  

- Complicated tests are hard to read and maintain, reducing their value. Their title should describe their sole purpose clearly.  
- Testing for the sake of testing doesn't provide value. 
- Running the test doesn't give you any confidence that there're no bugs because the test doesn't really do anything.  
- Testing complex areas and subcutaneous/integration tests are more high value tests.  
- Don't want really brittle tests either. Asserting absolutely everything means that one change can break unrelated tests.
- Deleting tests that are no longer useful is OK. If they add no value, they’re tech debt.  

The video asserts that the point of testing is to allow you to gain enough confidence to release to production.  

As I read through existing tests and go to write new tests, I’ll have my testing glasses on a bit more and see the tests that add value and the tests that are a bit heavier than they need to be.