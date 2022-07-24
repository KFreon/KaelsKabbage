// DOCS
// https://developer.chrome.com/docs/workbox/caching-strategies-overview/
// https://www.netlify.com/blog/2017/10/31/service-workers-explained/

// Using Stale-while-revalidate
// Return from cache if present (otherwise go to network)
// REGARDLESS go to network and update cache for next time




const cacheName = "KabbageCache";
const offlinePageUrl = "offline/index.html";

const shellFiles = [
  "index.html", "404.html",
  "fonts/FiraCode-Regular.woff", "fonts/FiraCode-Regular.woff2", 
  "img/BlenderIcon.png", "img/Cabbage.png", "img/Cabbage_Shortcut.png"];

self.addEventListener("install", function(event) {
  // Perform install steps
  console.log("SW Install");
  event.waitUntil(
    caches.open(cacheName).then(function(cache) {
      console.log("SW Caching app shell");
      return cache.addAll(shellFiles);
    })
  );
});

self.addEventListener("activate", function(event) {
  console.log("SW Activate");
  event.waitUntil(
    caches.keys().then(function(keyList) {
      return Promise.all(keyList.map(function(key) {
        if (key !== cacheName) {
          console.log("SW Removing old cache shell", key);
          return caches.delete(key);
        }
      }));
    })
  );
});

self.addEventListener("fetch", (event) => {
  if (event.request && (
    event.request.url.includes('googletagmanager') ||
    event.request.url.includes('chrome-extension')
    )) {
    return;
  }

  // Offline based on: https://googlechrome.github.io/samples/service-worker/custom-offline-page/
  event.respondWith((async() => {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(event.request);
    const promise = getNetworkResponse(cache, event);
    if (!cachedResponse) {
      return await promise;
    } else {
      return cachedResponse;
    }    
  })())
})

getNetworkResponse = async (cache, ev) => {
  try {
    const networkResponse = await fetch(ev.request);

    // Can't cache partial responses 206 (i.e. lazy videos)
    if ([200, 304].includes(networkResponse.status)) {
      cache.put(ev.request, networkResponse.clone());
    }

    return networkResponse;
  }
  catch{
    const offlinePage = await cache.match(offlinePageUrl);

    // There's an issue atm where you're not allowed to redirect unexpectedly, so we need to build a new response without redirect flag.
    // https://stackoverflow.com/questions/45434470/only-in-chrome-service-worker-a-redirected-response-was-used-for-a-reque

    return new Response(offlinePage.body, {
      headers: offlinePage.headers,
      status: offlinePage.status,
      statusText: offlinePage.statusText,
    });
  }
}