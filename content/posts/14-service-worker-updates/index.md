---
title: "Service Worker Updates"
date: 2019-06-04T15:51:30+10:00
draft: false
type: "post"
slug: service-worker-updates
tags: ["pwa", "service-worker"]
---

Service workers are the new hotness when it comes to web applications.  
They allow a website to have some offline behaviours similar to that of an app, including an "installable" desktop experience.  
In essence, they're a small app that browsers can interpret and run, caching info and queueing requests, enabling offline access and data-saving.

<!--more-->

**tl;dr** Updates are not automatic in many cases and can be forced by sending the `skipWaiting` message to the service worker, however it can have it's issues.  

There are really good explanations and examples of the hows and whys of service workers so I won't retread most of it.

### Info  
- [Service Worker Lifecycle](https://developers.google.com/web/fundamentals/primers/service-workers/lifecycle)  
- [Mozilla](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API/Using_Service_Workers)

### Potentially useful blog posts  
- [Stuff I Wish I'd known sooner](https://gist.github.com/Rich-Harris/fd6c3c73e6e707e312d7c5d7d0f3b2f9)  
- [SW's break the Refresh button](https://redfin.engineering/service-workers-break-the-browsers-refresh-button-by-default-here-s-why-56f9417694)
- [Service Worker Gotchas](https://novemberfive.co/blog/mess-up-service-workers-caching-gotcha)

This post is mostly a quick overview of the service worker and getting it to update.   

I've been working with an SPA app using [workbox](https://developers.google.com/web/tools/workbox/) which is the Google implementation of the service worker idea.  
It comes with some [Strategies](https://developers.google.com/web/tools/workbox/modules/workbox-strategies), termed such because they decide how the service worker should react in different situations, for example:  

- Online but the developers have explicitly indicated that this asset doesn't change often, so just return the cached item  
- Offline, but the asset changes all the time, so show an error/some message instead of returning cached values  

# Strategies

#### StaleWhileRevalidate  
- Return the cached response immediately (if exists) while going to the network to get updates (if any)  
- Common strategy
- Best used when the most up-to-date info isn't critical 

#### Network Only  
- Only go to the network (don't cache)  

#### Network First  
- Go to the network and use that to update the cache (for the most up to date detail, but works offline)  

#### Cache First  
- Returns the cached item until expiry or if not in the cache go to network (good for slow changing assets like logos, long living datablocks, etc)  

Service workers can manage some or all of the routes in your application. There's no need for all routes to be managed if that route won't benefit from service workers.  

# Example
At this point, let's look at an example.  
Workbox give us two files:  

- src/sw/sw.ts  
- src/serviceWorker.ts  

`sw.ts (or sw.js)` defines the service worker itself; It's purview over routes, strategies, cache behaviours, etc.  
It's the external bit that runs outside your webpage.  

`ServiceWorker.ts` controls the behaviour of the service worker from the webpage; Registration and deregistration logic, which essentially controls installation and updates.  

The below is from the generated files from create-react-app (using Workbox)

{{< splitter >}}
{{% split side=left title="sw.ts" %}}
``` js
workbox.precaching.precacheAndRoute(['/index.html'], {});

// Single url to be used for navigation in our SPA
workbox.routing.registerNavigationRoute(workbox.precaching.getCacheKeyForURL('/index.html'), {
  blacklist: [
    new RegExp('^/api/'), // Exclude URLs starting with /api/, as they're API calls (not navigation)
    new RegExp('/[^/]+\\.[^/]+$'), // Exclude URLs containing a dot, as they're likely a resource in public/ and not a SPA route
  ],
});

workbox.routing.registerRoute(
  new RegExp('/api/assets/.*'),
  new workbox.strategies.NetworkOnly({
    cacheName: 'assetCache',
    plugins: [
      new workbox.expiration.Plugin({
        maxEntries: 1,
        maxAgeSeconds: SEC_24HOURS,
      }),
      new workbox.backgroundSync.Plugin('assetQueue', {
        maxRetentionTime: SEC_24HOURS,
        onSync: onSync
      }),
    ],
  }),
  'POST'
);
```
{{% /split %}}
{{% split side=right title="serviceWorker.ts" %}}
``` js
window.addEventListener('load', () => {
    const swUrl = `${process.env.PUBLIC_URL}/sw.js`;

    if (isLocalhost) {
    // This is running on localhost. Let's check if a service worker still exists or not.
    checkValidServiceWorker(swUrl, config);

    // Add some additional logging to localhost, pointing developers to the
    // service worker/PWA documentation.
    navigator.serviceWorker.ready.then(() => {
        console.log(
        'This web app is being served cache-first by a service ' +
            'worker. To learn more, visit http://bit.ly/CRA-PWA'
        );
    });
    } else {
    // Is not localhost. Just register service worker
    registerValidSW(swUrl, config);
    }
});

function registerValidSW(swUrl: string, config?: Config) {
  navigator.serviceWorker
    .register(swUrl)
    .then(registration => {
      // Service worker is checked every 24h-ish or navigation (non-SPA)
      // We don't have any non-SPA navigations, so updates are rarely checked.
      // Force a proper update check at a set interval
      swRefreshIntervalHandle = window.setInterval(() => {
        console.log('Checking for service worker update...');
        registration.update();
      }, swRefeshIntervalInMs);

      registration.onupdatefound = () => {
        const installingWorker = registration.installing;
        if (installingWorker == null) {
          return;
        }
        installingWorker.onstatechange = () => {
          if (installingWorker.state === 'installed') {
            if (navigator.serviceWorker.controller) {
              // At this point, the updated precached content has been fetched,
              // but the previous service worker will still serve the older
              // content until all client tabs are closed.
              console.log(
                'New content is available and will be used when all ' +
                  'tabs for this page are closed. See http://bit.ly/CRA-PWA.'
              );

              // Execute callback
              if (config && config.onUpdate) {
                config.onUpdate(registration);
              }
            } else {
              // At this point, everything has been precached.
              // It's the perfect time to display a
              // "Content is cached for offline use." message.
              console.log('Content is cached for offline use.');

              // Execute callback
              if (config && config.onSuccess) {
                config.onSuccess(registration);
              }
            }
          }
        };
      };
    })
    .catch(error => {
      console.error('Error during service worker registration:', error);
    });
}
```
{{% /split %}}
{{% /splitter %}}


Quick explanations are the best explanations:  

- Precache `index.html` i.e. download it immediately and store it for offline access (might not be able to do much, but it ensures that something can be shown offline)
- Register a navigation route for the SPA. This means that there's a single page to be served for many sub-routes, which is `index.html` in this case

> We're blacklisting some routes as we don't want them to be considered navigations within the SPA and thus treated with `index.html` 

- Register all routes under `/api/assets` as NetworkOnly. Results for these routes are never returned by the cache
  - *I don't fully understand why the `cacheName` and expiration are there*
  - [BackgroundSync](https://developers.google.com/web/tools/workbox/modules/workbox-background-sync) handles the queueing of requests made when the network is unavailable. These requests are stored in local IndexDB and replayed when the service worker detects a network connection.  
- Also note that this particular example is for a `POST`. That can be omitted for a `GET` and BackgroundSync is not valid (since there's nothing to queue)

The `ServiceWorker.ts` example is incomplete to reduce the reading, and I won't go line by line. 
We register a window handler to register the service worker, with some different behaviour when running locally.  
The registration handler does the "installation" of the worker. In this case, I've also extended it to handle the update process.  

On the note of updates, let's move on...  

# Updating!! 
Updates for service workers are a bit tricky.  
Browsers check every now and then automatically, and when navigating to a managed route (I think? I didn't get much out of the docs from this)  
For this SPA app, it doesn't get closed much, meaning updates were at the mercy of the browsers' "every now and then" (seems to be 24h-ish)  

We wanted to reduce this time period and the uncertainty around it, so we used the manual update method of `registration.Update()`.  
Calling this method gets the service worker file (sw.js) from the server and checks to see if it's different to the current one. If it is, it is installed and put into the *waiting* phase.  

The waiting phase means the worker is installed successfully but the old worker is still handling requests; The new worker isn't yet "active".  
This behaviour is desired to prevent client side state issues, like "what if a worker has a different version of a Mobx model", etc.  
In order to get the new worker to take over, all windows/tabs must be closed (F5 isn't sufficient)  

For a SPA, this wasn't desirable, and we implemented an auto update system. On registration, the worker sets up a recurring timer to check for updates (6h interval).  
If one is found, it's installed as per usual, but also registers a route handler such that when the app is on the home/login page (where state is irrelevant), kick the old worker out.  

Service workers can be communicated to and from the parent website via messages/events (terminology seems to be in flux).  
`SkipWaiting` is one of the messages that can be communicated and it tells the browser/service worker combo to kick out the old worker and activate the new one immediately.  

In the service worker file (sw.ts) we register a event handler to listen for messages, specifically `skipWaiting`.   
We also pass it the route listener from that posts the `skipWaiting` message when the route matches.  

{{< splitter >}}
{{% split side=left title="sw.ts" %}}
```js
// This runs in the service worker. We listen for the 'skipWaiting'.
// SkipWaiting means "kick the current worker out and have this one take over"
// This updates the service worker without requiring all windows to be closed.
self.addEventListener('message', (event: ExtendableMessageEvent) => {
  if (event.data === 'skipWaiting') {
    self.skipWaiting();
  }
});
```
{{% /split %}}
{{% split side=right title="serviceWorker.ts" %}}
```js
// When a new service worker is ready to be installed, wait until we're on the login page
// Setup a refresh for when the new service worker takes over to refresh the page for new content
// Then tell the service worker it can kick the old one out
// THEN we need to refresh the window so it detects something has changed before it can do the change it knows about...
function onUpdate(registration: ServiceWorkerRegistration, store: IRootStoreModel) {
  registration.waiting!.onstatechange = _ => window.location.reload();

  store.history.listenForRoute(_ => {
    console.log("Home route, updating service worker");
    registration.waiting!.postMessage("skipWaiting");
    window.location.reload();
  } , { path: '/home', exact: true });
}
```
{{% /split  %}}
{{% /splitter %}}

> NOTE that the page is refreshed on service worker update. This is to ensure that stylesheets and html changes are immediately visible (cached items will be revalidated on registration of the service worker in this case)

# Issues
One issue we had was local testing. CRA doesn't run the service worker when doing an `npm start` as there isn't really a server for it to run on. See the [documentation](https://facebook.github.io/create-react-app/docs/making-a-progressive-web-app) for info.   
The workaround was to do `npm build` and setup a local server using `http-server` and run it from there.  


The fun one that I didn't have to worry about was the one where you cache something too aggressively and can't get the service worker to behave.  

Getting your head around the service worker lifecycle is essential to understand when changes you've deployed actually come to light.  
The Chrome Dev Tools can provide some assistance here, allowing manual update checking, forced installations, etc  

{{< image path="img/ServiceWorker_DevTools" alt="Service Worker area in Chrome Dev Tools" >}}

This was all done on Chrome ~74-ish and an Android tablet.  