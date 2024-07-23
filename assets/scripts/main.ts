import { handleEdgeAV1Support, imageContainerClicked, setupImageSizes, setupLazyVideos, zoomImage } from "./MediaScripts";
import { hideBigSearch, insideSearchTextbox, queueSearch, setupSearch, showBigSearch } from "./Search";
export type RenderDisplayType = 'slides' | 'tiles' | 'carousel'
export type Theme = 'light' | 'dark'

const theme = localStorage.getItem('theme') as Theme | undefined;

export function setRenderDisplay(displayType: RenderDisplayType | undefined) {
  // Clear all first
  const element = document.getElementById("slides")

  element?.classList.remove("tiles");
  element?.classList.remove("slides");

  switch (displayType) {
    case 'tiles':
      element?.classList.add("tiles");
      break;
    case 'carousel':
      element?.classList.add("slides");
      break;
    default:
      element?.classList.add("tiles");
      break;
  }
  localStorage.setItem('render-display', displayType);
}

export function setTheme(theme: Theme | undefined) {
  const body = document.getElementById("main-body")!;
  if (theme === 'light') {
    body.className = 'light';
  } else {
    body.className = 'dark';
  }
}

export function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}

export function toggleHamburger() {
  document.getElementsByClassName("header-menu-container")[0].classList.toggle("open");
  
  document.getElementsByTagName("footer")[0].classList.toggle("open");
}

export function getTagFromQueryString() {
  const queryTag = window.location.search.slice(1);
  if (!queryTag) return;
  
  const matchingElement = document.getElementById(queryTag);
  if (matchingElement) {
    matchingElement.classList.add('highlighted-tag');
    matchingElement.scrollIntoView();
  }
}

// Lame, but that's how esbuild works
function setupFunctionsInHtml() {
  //@ts-ignore
  window.zoomImage = zoomImage;
  //@ts-ignore
  window.imageContainerClicked = imageContainerClicked;
  //@ts-ignore
  window.toggleTheme = toggleTheme;
  //@ts-ignore
  window.showBigSearch = showBigSearch;
  //@ts-ignore
  window.toggleHamburger = toggleHamburger;
  //@ts-ignore
  window.hideBigSearch = hideBigSearch;
  //@ts-ignore
  window.insideSearchTextbox = insideSearchTextbox;
  //@ts-ignore
  window.queueSearch = queueSearch;
}

function atStartup() {
  setupFunctionsInHtml();

  setupLazyVideos();
  getTagFromQueryString();

  setTheme(theme);

  setupSearch();
  
  if (document.getElementById("render-tiles-button")) {
    const renderDisplay = localStorage.getItem('render-display') as RenderDisplayType | undefined;
    setRenderDisplay(renderDisplay);
  }

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

  setTimeout(() => {
    setupImageSizes()
  }, 100);

  setTimeout(() => {
    handleEdgeAV1Support();
  }, 1000);
}

atStartup();