const theme = localStorage.getItem('theme');
setTheme(theme);

if (document.getElementById("render-tiles-button")) {
  const renderDisplay = localStorage.getItem('render-display');
  setRenderDisplay(renderDisplay);
}
