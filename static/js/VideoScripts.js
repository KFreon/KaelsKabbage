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

  console.log('half', showHalfsize, 'quarter', showQuartersize)

  // normal, half, quarter
  let mediaSizeType = showQuartersize ? 'quarter' : showHalfsize ? 'half' : 'normal';
  if (isRenderList) {
    mediaSizeType = 'quarter';
  }

  for(let item of media) {
    item.style.display = mediaSizeType === 'normal' ? 'block' : 'none';
  };
  for(let half of halfsize) {
    half.style.display = mediaSizeType === 'half' ? 'block' : 'none';
  };
  for(let half of quartersize) {
    half.style.display = mediaSizeType === 'quarter' ? 'block' : 'none';
  };
}, 100);

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
  const isEdge = navigator.userAgentData?.brands?.some(b => b.brand === 'Microsoft Edge');
  if (!isEdge) {
    return;
  }

  const rawVideos = document.getElementsByTagName("video");
  const videos = rawVideos && Array.from(rawVideos);
  const edgeSupportsAV1 = videos && videos.some(v => v.webkitDecodedFrameCount > 0);

  if (videos && !edgeSupportsAV1) {
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

setupLazyVideos();