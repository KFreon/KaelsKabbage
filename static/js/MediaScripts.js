// this is so dumb but I can't figure out a better way
// I want to show halfsize on the list page, but it just displays the content of the subpages.
// There doesn't seem to be a media query that can work on the calculated width of the resulting image.
// i.e. If the image is going to be small, use the small image. I can't figure out how to know that in advance.
// It can know what the viewport size is, but not the resulting image.
// So I've created two elements, and toggle between them here.
// The reason it has to be a timer instead of just normal CSS is that it still renders some of them.
// I think it's a timing thing where the default visibility is being fetched before it knows the element isn't visible.
setTimeout(() => {

  const media = document.getElementsByClassName('media-frame');

  const halfsize = document.getElementsByClassName('halfsize-frame');
  const quartersize = document.getElementsByClassName('quartersize-frame');
  const renderList = document.getElementsByClassName("render-list");
  const isRenderList = renderList && renderList.length > 0;
  const showHalfsize = document.documentElement.clientWidth <= 1200;
  const showQuartersize = document.documentElement.clientWidth <= 800;

  // normal, half, quarter
  let mediaSizeType = showQuartersize ? 'quarter' : showHalfsize ? 'half' : 'normal';
  if (isRenderList) {
    mediaSizeType = 'quarter';
  }

  setMediaSizeDisplayVisibility(media, mediaSizeType === 'normal')
  setMediaSizeDisplayVisibility(halfsize, mediaSizeType === 'half')
  setMediaSizeDisplayVisibility(quartersize, mediaSizeType === 'quarter')
}, 100);

function setMediaSizeDisplayVisibility(medias, isVisible) {
  for(let item of medias) {
    item.style.display = isVisible ? 'block' : 'none';
  };
}

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
  
  const imageCenterWidth = width / 2
  const viewCenterWidth = viewWidth / 2
  const imageAbsoluteCenterWidth = imageCenterWidth + left;
  const widthDiff = viewCenterWidth - imageAbsoluteCenterWidth;

  const imageCenterHeight = height / 2
  const viewCenterHeight = viewHeight / 2
  const imageAbsoluteCenterHeight = imageCenterHeight + top - 50;  // top buffer
  const heightDiff = viewCenterHeight - imageAbsoluteCenterHeight;

  const xscale = viewWidth / width;
  const yscale = viewHeight / height;
  const scale = Math.min(xscale, yscale);

  const widthDiffScaled = widthDiff / scale
  const heightDiffScaled = heightDiff / scale

  const originalStyle = {
    transform: 'scale(1) translate(0px)',
    zIndex: e.metaZ,
    boxShadow: '',
    background: 'transparent'
  }

  const zoomedStyle = {
    transform: `scale(${scale} translate(${widthDiffScaled}px, ${heightDiffScaled}px)`,
    zIndex: 9999,
    boxShadow: '0 0 10px 10px black',
    background: 'black'
  }

  // z-index chained after transforms finished?

  // Set saved zIndex if required
  if (e.style.zIndex != maxZIndex) {
    e.metaZ = e.style.zIndex;
  }

  if (!e.style.transform || e.style.transform === originalTransform) {
    e.style = {...e.style, ...zoomedStyle}
  } else {
    e.style = {...e.style, ...originalStyle}
  }
}