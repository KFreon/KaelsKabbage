function setMediaSizeDisplayVisibility(medias: HTMLCollectionOf<HTMLElement>, isVisible: boolean) {
  // @ts-ignore
  for(let item of medias) {
    item.style.display = isVisible ? 'block' : 'none';
  };
}

function removeAV1VideoSource() {
  // @ts-ignore
  for (let element of document.querySelectorAll("video>source[type*='av01.']")) {
    let parentElement = element.parentElement as HTMLVideoElement;

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
  // @ts-ignore
  if (navigator.userAgentData) {
    // @ts-ignore
    isEdge = navigator.userAgentData?.brands?.some(b => b.brand === 'Microsoft Edge');
  }

  if (!isEdge) {
    return;
  }

  const rawVideos = document.getElementsByTagName("video");
  const videos = rawVideos && Array.from(rawVideos);
  // @ts-ignore
  const edgeSupportsAV1 = videos && videos.some(v => v.webkitDecodedFrameCount > 0);

  if (videos && videos.length > 0 && !edgeSupportsAV1) {
    console.warn('edge: av1 not supported')
    removeAV1VideoSource();

    // Display edge message
    document.getElementById("edge-av1-message")!.style.display = "block";
  }
}

function imageContainerClicked(url: string) {
  // Ignore if not in a render list
  const renderList = document.getElementsByClassName("render-list");
  const isRenderList = renderList && renderList.length > 0;
  if (isRenderList)  {
    location.href = url;
  }
}

function setupLazyVideos() {
  document.addEventListener("DOMContentLoaded", function() {
    let lazyVideos = document.querySelectorAll("video.lazy");
  
    if ("IntersectionObserver" in window) {
      let lazyVideoObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(video) {
          if (video.isIntersecting) {
            for (let source in video.target.children) {
              let videoSource = video.target.children[source] as HTMLVideoElement;
              if (typeof videoSource.tagName === "string" && videoSource.tagName === "SOURCE") {
                videoSource.src = videoSource.dataset.src!;
              }
            }

            (video.target as HTMLVideoElement).load();
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

function zoomImage(e: HTMLElement & { metaZ: string }) {
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
  if (+e.style.zIndex != zoomedStyle.zIndex) {
    // Note if we leave it as undefined or 0 (auto) then the sidebar is automatically on top
    // And the shrink animation is weird
    e.metaZ = e.style.zIndex || "1"
  }

  const targetStyle = (!e.style.transform || e.style.transform === originalStyle.transform) ? zoomedStyle : originalStyle;

  // Can't do spread for some reason :(
  Object.assign(e.style, targetStyle);
}

// this is so dumb but I can't figure out a better way
// I want to show halfsize on the list page, but it just displays the content of the subpages.
// There doesn't seem to be a media query that can work on the calculated width of the resulting image.
// i.e. If the image is going to be small, use the small image. I can't figure out how to know that in advance.
// It can know what the viewport size is, but not the resulting image.
// So I've created two elements, and toggle between them here.
// The reason it has to be a timer instead of just normal CSS is that it still renders some of them.
// I think it's a timing thing where the default visibility is being fetched before it knows the element isn't visible.
setTimeout(() => {

  const media = document.getElementsByClassName('media-frame') as HTMLCollectionOf<HTMLElement>;

  const halfsize = document.getElementsByClassName('halfsize-frame') as HTMLCollectionOf<HTMLElement>;
  const quartersize = document.getElementsByClassName('quartersize-frame') as HTMLCollectionOf<HTMLElement>;
  const renderList = document.getElementsByClassName("render-list");
  const isRenderList = renderList && renderList.length > 0;
  const showHalfsize = document.documentElement.clientWidth <= 1200;
  const showQuartersize = document.documentElement.clientWidth <= 800;

  
  // normal, half, quarter
  let mediaSizeType = showQuartersize ? 'quarter' : showHalfsize ? 'half' : 'normal';
  if (isRenderList) {
    mediaSizeType = 'quarter';
  }

  // TODO: Add other sizes instead.
  const anyOtherSizes = halfsize.length > 0 || quartersize.length > 0;

  setMediaSizeDisplayVisibility(media, mediaSizeType === 'normal' || !anyOtherSizes)
  setMediaSizeDisplayVisibility(halfsize, mediaSizeType === 'half')
  setMediaSizeDisplayVisibility(quartersize, mediaSizeType === 'quarter')
}, 100);

setTimeout(() => {
  handleEdgeAV1Support();
}, 1000);