---
title: "Aspnetcore - A multiple authentication scheme story"
date: 2023-08-29T07:27:03+10:00
type: "post"
slug: "aspnetcore--multiple-authentication-schemes"
tags: ["auth", "aspnetcore"]
aliases: aspnetcore-multiple-authentication-schemes
---

Many of the Aspnetcore auth samples and tutorials discuss using multiple authentication schemes, but I ran into an interesting issue that I found interesting.  
It's also obvious in retrospect 😭  
In short: I had multiple schemes working fine, but the default scheme always ran (even though it failed) and logged failure messages.   
Why?  

<!--more-->  

<div style="margin: auto;width:20%;min-width:40rem;">
	<div class="tenor-gif-embed" data-postid="15779151" data-share-method="host" data-aspect-ratio="1.33333" data-width="100%"><a href="https://tenor.com/view/sheldon-oh-frickity-frack-not-this-again-gif-15779151">Sheldon Oh GIF</a>from <a href="https://tenor.com/search/sheldon-gifs">Sheldon GIFs</a></div> <script type="text/javascript" async src="https://tenor.com/embed.js"></script>
</div>

# In short  
## The issue  
When a scheme was requested specifically with `[Authorize(SimpleOptions.Name)]`, the default scheme ALSO ran.    
The default authentication would fail but since it wasn't the requested policy, it was allowed to fail.  
However it was writing logs on failure, and I wanted to prevent that.  

But...why is it running the default when I asked for the `Simple` scheme?   

## The reason?  
The short answer is: I'm not 100% sure, but it seems the authentication happens before any authorization is considered.  
As such, if there's a default authentication, it will get run.  
Then it gets to the authorization part, which can request an authentication scheme as well, and it can ignore the default result if there's another one.  

## My solution  
This is difficult to explain in shortform, but in my case, the solution was:
- Remove default authentication scheme  
- Ensure all authorization policies that require authentication also specifies at least one authentication scheme  
- Use `FallbackPolicy` instead of default  

# Much more info  
Here, I'll go into more detail about my setup and what I tried and what happened when it failed.  
As with all auth blog posts, this'll be out of date tomorrow anyway, but it'll be here to confuse someone down the track (most likely me)  

# My auth setup and why  
This was part of a migration project, and they didn't use `[Authorize]` and I didn't want to add it across the board.  
As such, I only use `[Authorize(SCHEME)]` when I want a non-default policy, and `[AllowAnonymous]` for no auth.  

## Original  
{{% splitter %}}
{{% split side=left title="Original" %}}
```cs
// Note that in NET 7, if there's only one scheme, it will still be registered as the default if omitted.  
services.AddAuthentication(PrimaryOptions.Name) 
	.AddPolicy<PrimaryHandler, PrimaryOptions>(PrimaryOptions.Name, opts => {})
	.AddPolicy<SimpleHandler, SimpleOptions>(SimpleOptions.Name, opts => {});

services.AddAuthorization(authBuilder => {
authBuilder.AddPolicy(SimplePolicy.Name, policy => 
		policy
			.RequireAuthenticatedUser());

	authBuilder.AddPolicy(PrimaryOptions.Name, policy => 
		policy
			.AddAuthenticationSchemes(PrimaryOptions.Name)
			.RequireAuthenticatedUser());
	
  // Use fallback as the default
	authBuilder.FallbackPolicy = authBuilder.GetPolicy(PrimaryOptions.Name);
});
```   
{{% /split %}}
{{% split side=right title="Fixed" %}}
```cs
// Note that in NET 7, if there's only one scheme, it will still be registered as the default if omitted.  
services.AddAuthentication()  // NO default 
	.AddPolicy<PrimaryHandler, PrimaryOptions>(PrimaryOptions.Name, opts => {})
	.AddPolicy<SimpleHandler, SimpleOptions>(SimpleOptions.Name, opts => {});

services.AddAuthorization(authBuilder => {
authBuilder.AddPolicy(SimplePolicy.Name, policy => 
		policy
			.AddAuthenticationSchemes(SimpleOptions.Name)  // Added this!
			.RequireAuthenticatedUser());

	authBuilder.AddPolicy(PrimaryOptions.Name, policy => 
		policy
			.AddAuthenticationSchemes(PrimaryOptions.Name)
			.RequireAuthenticatedUser());
	
  // Use fallback as the default
	authBuilder.FallbackPolicy = authBuilder.GetPolicy(PrimaryOptions.Name);
});
```   
{{% /split %}}
{{< /splitter >}}  

Let's go through a couple of things from above.

## Empty AddAuthentication()
`AddAuthentication()` indicates there's no default authentication scheme registered.  
With an empty `[Authorize]` attribute (either explicit or added by `.RequireAuthorization()`) we get:
> No authenticationScheme was specified and no DefaultChallengeScheme found.  

This essentially disables auth on endpoints that don't have an authorize attribute...so I'd need to add `[Authorize(PrimaryOptions.Name)]` or specify the policy in `.RequireAuthorization(MAIN)`?  
I don't want to do that!  
Fortunately, there is another option: `FallbackPolicy`.  

## Fallback policy  
This applies across the board when there is no other policy specified.  
It feels more like the "default" to me, as it will be the one that's run when nothing else is indicated.  
To be clear: Without `[Authorize]`, the fallback is run, but with `[Authorize]` and NO scheme specified, we get the same "No authenticationScheme was specified..."  

## Don't forget the auth scheme in the auth policy 🤔
Everything was working fine, only the requested schemes running, etc EXCEPT for a couple of endpoints which still gave me the "No authentication scheme was specified...".  
Turns out, I had an authorisation policy that required authentication but didn't specify which scheme and there was no default 😭
I was using it like: `[Authorize(SimpleOptions.Name)]`; a policy was specified, so the fallback scheme doesn't apply, but no other schemes are registered either, hence the error.

As such, the fix is just to add an authentication scheme for that policy.  
So simple and obvious in retrospect 🤦