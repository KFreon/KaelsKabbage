function setRenderDisplay(displayType) {
  // Clear all first

  document.getElementById("slides").classList.remove("tiles");
  document.getElementById("slides").classList.remove("slides");

  switch (displayType) {
    case 'tiles':
      document.getElementById("slides").classList.add("tiles");
      break;
    case 'carousel':
      document.getElementById("slides").classList.add("slides");
      break;
    default:
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
  if (!queryTag) return;
  
  const matchingElement = document.getElementById(queryTag);
  if (matchingElement) {
    matchingElement.classList.add('highlighted-tag');
    matchingElement.scrollIntoView();
  }
}