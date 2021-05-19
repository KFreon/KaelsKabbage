function setRenderDisplay(displayType) {
  // Clear all first
  document.getElementById("render-list-button").classList.remove("selected");
  document.getElementById("render-tiles-button").classList.remove("selected");
  document.getElementById("render-carousel-button").classList.remove("selected");

  document.getElementById("slides").classList.remove("tiles");
  document.getElementById("slides").classList.remove("slides");

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
    document.getElementById("dark-mode").disabled = false;
  } else {
    document.getElementById("dark-mode").disabled = true;
  }
}

function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}