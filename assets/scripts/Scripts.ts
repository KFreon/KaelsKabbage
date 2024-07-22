function setRenderDisplay(displayType) {
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

function setTheme(theme) {
  const body = document.getElementById("main-body")!;
  if (theme === 'light') {
    body.className = 'light';
  } else {
    body.className = 'dark';
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
  if (!queryTag) return;
  
  const matchingElement = document.getElementById(queryTag);
  if (matchingElement) {
    matchingElement.classList.add('highlighted-tag');
    matchingElement.scrollIntoView();
  }
}