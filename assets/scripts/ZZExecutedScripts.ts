// MUST be Loaded last
setupLazyVideos();
getTagFromQueryString();

if (window.location.host !== "localhost:1313") {
  // Not using the service worker for now
  //registerServiceWorker();

  // Unregister any service workers for this
  navigator.serviceWorker.getRegistrations().then(function(registrations) {
    for(let registration of registrations) {
      registration.unregister();
    }
  });
}