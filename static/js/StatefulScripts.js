const theme = localStorage.getItem('theme');
setTheme(theme);

if (document.getElementById("render-list-button")) {
  const renderDisplay = localStorage.getItem('render-display');
  setRenderDisplay(renderDisplay);
}
