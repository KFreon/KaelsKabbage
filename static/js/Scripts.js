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