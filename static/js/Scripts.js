function setRenderDisplay(displayType) {
  // Clear all first
  document.getElementById("render-tiles-button").classList.remove("selected");
  document.getElementById("render-carousel-button").classList.remove("selected");

  document.getElementById("slides").classList.remove("tiles");

  switch (displayType) {
    case 'list':
      document.getElementById("render-list-button").classList.add("selected");
      break;
    case 'tiles':
      document.getElementById("render-tiles-button").classList.add("selected");
      document.getElementById("slides").classList.add("tiles");
      break;
    case 'carousel':
      document.getElementById("render-carousel-button").classList.add("selected");
      document.getElementById("slides").classList.add("slides");
      break;
    default:
      break;
  }
  localStorage.setItem('render-display', displayType);
}

function setTheme(theme) {
  if (theme === 'dark') {
    document.getElementById("main-body").className = 'dark';
  } else {
    document.getElementById("main-body").className = 'light';
  }
}

function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}

function imageClicked(element) {
  const isOpen = element.classList.contains('open');
  if (isOpen) {
    element.classList.remove('open');
  } else {
    element.classList.add('open')
  }
}