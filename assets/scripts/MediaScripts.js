setTimeout(() => {
  handleEdgeAV1Support();
}, 1000);

function removeAV1VideoSource() {
  for (let element of document.querySelectorAll("video>source[type*='av01.']")) {
    let parentElement = element.parentElement;

    // Remove the <source> element
    element.remove();

    // force reload the video, so the browser loads the new sources
    parentElement.load();
  }
}

// Edge doesn't support AV1 natively
// If the extension is installed, use it.
// If not, remove it
function handleEdgeAV1Support() {
  let isEdge = false;
  if (navigator.userAgentData) {
    isEdge = navigator.userAgentData?.brands?.some(b => b.brand === 'Microsoft Edge');
  }

  if (!isEdge) {
    return;
  }

  const rawVideos = document.getElementsByTagName("video");
  const videos = rawVideos && Array.from(rawVideos);
  const edgeSupportsAV1 = videos && videos.some(v => v.webkitDecodedFrameCount > 0);

  if (videos && videos.length > 0 && !edgeSupportsAV1) {
    console.warn('edge: av1 not supported')
    removeAV1VideoSource();

    // Display edge message
    document.getElementById("edge-av1-message").style.display = "block";
  }
}

function imageContainerClicked(url) {
  // Ignore if not in a render list
  const renderList = document.getElementsByClassName("render-list");
  const isRenderList = renderList && renderList.length > 0;
  if (isRenderList)  {
    location.href = url;
  }
}

function setupLazyVideos() {
  document.addEventListener("DOMContentLoaded", function() {
    let lazyVideos = [].slice.call(document.querySelectorAll("video.lazy"));
  
    if ("IntersectionObserver" in window) {
      let lazyVideoObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(video) {
          if (video.isIntersecting) {
            for (let source in video.target.children) {
              let videoSource = video.target.children[source];
              if (typeof videoSource.tagName === "string" && videoSource.tagName === "SOURCE") {
                videoSource.src = videoSource.dataset.src;
              }
            }

            video.target.load();
            video.target.classList.remove("lazy");
            lazyVideoObserver.unobserve(video.target);
          }
        });
      });
  
      lazyVideos.forEach(function(lazyVideo) {
        lazyVideoObserver.observe(lazyVideo);
      });
    }
  });
}

function zoomImage(e) {
  const {x, y, width, height, top} = e.getBoundingClientRect();
  const left = x;

  const viewWidth = window.innerWidth;
  const viewHeight = window.innerHeight;
  
  const imageCenterRelX = width / 2
  const viewCenterX = viewWidth / 2
  const imageAbsoluteCenterWidth = imageCenterRelX + left;
  const widthDiff = viewCenterX - imageAbsoluteCenterWidth;

  const imageCenterRelY = height / 2
  const viewCenterY = viewHeight / 2
  const imageAbsoluteCenterHeight = imageCenterRelY + top - 50;  // top buffer
  const heightDiff = viewCenterY - imageAbsoluteCenterHeight;

  const xscale = viewWidth / width;
  const yscale = viewHeight / height;


  // Renders scale is too big somehow...
  const scale = Math.min(xscale, yscale) - .1;

  const widthDiffScaled = widthDiff / scale
  const heightDiffScaled = heightDiff / scale

  const originalStyle = {
    transform: 'scale(1) translate(0px)',
    zIndex: e.metaZ,
    boxShadow: '',
    background: 'transparent'
  }

  const zoomedStyle = {
    transform: `scale(${scale}) translate(${widthDiffScaled}px, ${heightDiffScaled}px)`,
    zIndex: 9999,
    boxShadow: '0 0 40px 50px black',
    background: 'black'
  }

  // Set saved zIndex if required
  if (e.style.zIndex != zoomedStyle.zIndex) {
    // Note if we leave it as undefined or 0 (auto) then the sidebar is automatically on top
    // And the shrink animation is weird
    e.metaZ = e.style.zIndex || 1
  }

  const targetStyle = (!e.style.transform || e.style.transform === originalStyle.transform) ? zoomedStyle : originalStyle;

  // Can't do spread for some reason :(
  Object.assign(e.style, targetStyle);
}