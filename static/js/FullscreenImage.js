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
var imgs = document.getElementsByClassName("image");
for (let img of imgs) {
    img.onclick = function(){
        modal.style.display = "flex";
        modalImg.src = img.src;
    }
}