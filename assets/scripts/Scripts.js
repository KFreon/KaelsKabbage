function setRenderDisplay(displayType) {
  // Clear all first
  document.getElementById("render-tiles-button").classList.remove("selected");
  document.getElementById("render-carousel-button").classList.remove("selected");

  document.getElementById("slides").classList.remove("tiles");
  document.getElementById("slides").classList.remove("slides");

  switch (displayType) {
    case 'tiles':
      document.getElementById("render-tiles-button").classList.add("selected");
      document.getElementById("slides").classList.add("tiles");
      break;
    case 'carousel':
      document.getElementById("render-carousel-button").classList.add("selected");
      document.getElementById("slides").classList.add("slides");
      break;
    default:
      document.getElementById("render-carousel-button").classList.add("selected");
      document.getElementById("slides").classList.add("tiles");
      break;
  }
  localStorage.setItem('render-display', displayType);
}

function setTheme(theme) {
  if (theme === 'light') {
    document.getElementById("main-body").className = 'light';
  } else {
    document.getElementById("main-body").className = 'dark';
  }
}

function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}

function toggleHamburger() {
  document.getElementsByClassName("header-menu-container")[0].classList.toggle("open");
  
  document.getElementsByTagName("footer")[0].classList.toggle("open");
}

function getTagFromQueryString() {
  const queryTag = window.location.search.slice(1);
  const matchingElement = document.getElementById(queryTag);
  if (matchingElement) {
    matchingElement.classList.add('highlighted-tag');
    matchingElement.scrollIntoView();
  }
}