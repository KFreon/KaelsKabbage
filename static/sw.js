// DOCS
// https://developer.chrome.com/docs/workbox/caching-strategies-overview/
// https://www.netlify.com/blog/2017/10/31/service-workers-explained/

// Using Stale-while-revalidate
// Return from cache if present (otherwise go to network)
// REGARDLESS go to network and update cache for next time




const shellCacheName = "KabbageShell";
const shellFiles = [
  "index.html", "404.html", 
  "fonts/FiraCode-Regular.woff", "fonts/FiraCode-Regular.woff2", 
  "img/BlenderIcon.png", "img/Cabbage.png", "img/Cabbage_Shortcut.png"];
const cacheName = "KabbageCache";

self.addEventListener("install", function(event) {
  // Perform install steps
  console.log("SW Install");
  event.waitUntil(
    caches.open(shellCacheName).then(function(cache) {
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
        if (key !== shellCacheName) {
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

  event.respondWith(
    caches.open(cacheName).then((cache) => 
      cache.match(event.request).then((cachedResponse) => {
        const fetchedResponse = fetch(event.request).then((networkResponse) => {
          // Can't cache partial responses 206 (i.e. lazy videos)
          if ([200, 304].includes(networkResponse.status)) {
            cache.put(event.request, networkResponse.clone());
          }

          return networkResponse;
        });

        return cachedResponse || fetchedResponse;
      }))
  );
});