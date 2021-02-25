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
}