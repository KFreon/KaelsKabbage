function setRenderDisplay(displayType) {
  // Clear all first
  document.getElementById("render-tiles-button").classList.remove("selected");
  document.getElementById("render-carousel-button").classList.remove("selected");

  document.getElementById("slides").classList.remove("tiles");
  document.getElementById("slides").classList.remove("slides");

  switch (displayType) {
    case 'tiles':
      document.getElementById("render-tiles-button").classList.add("selected");
      document.getElementById("slides").classList.add("tiles");
      break;
    case 'carousel':
      document.getElementById("render-carousel-button").classList.add("selected");
      document.getElementById("slides").classList.add("slides");
      break;
    default:
      document.getElementById("render-carousel-button").classList.add("selected");
      document.getElementById("slides").classList.add("slides");
      break;
  }
  localStorage.setItem('render-display', displayType);
}

function setTheme(theme) {
  if (theme === 'dark') {
    document.getElementById("main-body").className = 'dark';
  } else {
    document.getElementById("main-body").className = 'light';
  }
}

function toggleTheme() {
  const currentTheme = localStorage.getItem('theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  localStorage.setItem('theme', newTheme);
  setTheme(newTheme);
}

function imageClicked(element) {
  const slidesElement = document.getElementById("slides");
  const img = element.getElementsByClassName('inner')[0];

  const isRender = !!slidesElement;

  if (slidesElement && slidesElement.className === 'tiles') {
    const target = img.src;
    window.location.href = target;
  } else {
    const isOpen = element.classList.contains('open');
    if (isOpen) {
      element.classList.remove('open');
      img.style = "";
    } else {
      let scale = 2;
      if (isRender) {
        const viewWidth = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0)
        const viewHeight = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0)
  
        const { width: imageWidth, height: imageHeight } = img.getBoundingClientRect();
  
        const widthDiff = viewWidth - imageWidth;
        const heightDiff = viewHeight - imageHeight;
        scale = widthDiff > heightDiff
          ? viewHeight / imageHeight
          : viewWidth / imageWidth;
      }

      img.style = `transform:scale(${scale - 0.05})`;
      element.classList.add('open')
    }
  }
}

function toggleHamburger() {
  document.getElementsByClassName("header-menu-container")[0].classList.toggle("open");
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

  // Note this is also videos
  const images = document.getElementsByClassName('picture-frame');
  const halfsize = document.getElementsByClassName('halfsize-frame');
  const renderList = document.getElementsByClassName("render-list");
  const isRenderList = renderList && renderList.length > 0;
  const isMobile = document.documentElement.clientWidth <= 960;

  for(let image of images) {
    image.style.display = isRenderList || isMobile ? 'none' : 'block';
  };
  for(let half of halfsize) {
    half.style.display = isRenderList || isMobile ? 'block' : 'none';
  };
}, 100);

function imageContainerClicked(url) {
  location.href = url;
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