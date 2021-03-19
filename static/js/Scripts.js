function setRenderDisplay(displayType) {
  // Clear all first
  $("#render-list-button").removeClass("selected");
  $("#render-tiles-button").removeClass("selected");
  $("#render-carousel-button").removeClass("selected");

  $("#slides").removeClass("tiles");
  $("#slides").removeClass("slides");

  switch (displayType) {
    case 'list':
      $("#render-list-button").addClass("selected");
      break;
    case 'tiles':
      $("#render-tiles-button").addClass("selected");
      $("#slides").addClass("tiles");
      break;
    case 'carousel':
      $("#render-carousel-button").addClass("selected");
      $("#slides").addClass("slides");
      break;
    default:
      break;
  }
  localStorage.setItem('render-display', displayType);
}

function setTheme(theme) {
  if (theme === 'dark') {
    $("#dark-mode").prop('disabled', false);
  } else {
    $("#dark-mode").prop('disabled', true);
  }
}

function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}