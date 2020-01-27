// Get the modal
var modal = document.getElementById("fullscreen-image-modal");
modal.onclick = function() {
  modal.style.display = "none";
}

window.onkeydown = function(event) {
    if (event.keyCode == 27) { // Esc
        modal.style.display = "none";
    }
}

var modalImg = document.getElementById("fullscreen-image");
var hqButton = document.getElementById("hq-button");
var imgs = document.getElementsByClassName("image");
for (let img of imgs) {
    img.onclick = function(){
        modalImg.style = null;
        if (hqButton) {
            hqButton.style.visibility = "visible";
        }
        modal.style.display = "flex";
        modalImg.src = img.currentSrc;
    }
}

fullscreenHQClick = function(event) {
    modalImg.style = "filter: blur(10px);";
    hqButton.style.visibility = "hidden";
    modalImg.src = modalImg.src.replace('webp', 'png');
    event.stopPropagation();
}

fullscreenLoading = function(event) {
    modalImg.style = null;
    event.stopPropagation();
}