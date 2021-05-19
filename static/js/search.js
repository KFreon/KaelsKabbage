let searchResults = [];
let resultsElement = document.getElementById('results');
let searchInputElement = document.getElementById('searchbox');

function showIfNecessary() {
    if (searchResults.length > 0) {
        toggleResults(true);
    }
}

function hideIfNecessary() {
    if (document.documentElement.clientWidth < 960) {   // CSS mobile breakpoint
        toggleResults(false); 
    }
}

function search() {
    // Clear
    while (resultsElement.firstChild) {
        resultsElement.removeChild(resultsElement.lastChild);
    }

    if (searchInputElement.value.length < 2) {
        toggleResults(false);
        return;
    }

    const query = searchInputElement.value.toUpperCase();
    searchResults = pagesIndex.filter(x => x.title.toUpperCase().includes(query));
    renderResults();
}

function renderResults() {
    if (!searchResults.length) {
        toggleResults(false);
        return;
    }

    toggleResults(true);

    // Only show the ten first results
    searchResults.slice(0, 4).forEach(function(result) {
        const text = `${result.isRender ? '🖼' : ''} ${result.title}`;
        const node = document.createElement("li");
        node.classList.add('search-result');

        const link = document.createElement('a');
        link.href = result.href;
        link.textContent = text;

        node.appendChild(link);
        resultsElement.appendChild(node);
    });
}

function toggleResults(isVisible) {
    if (isVisible) {
        resultsElement.classList.add("visible")
    } else {
        resultsElement.classList.remove("visible")        
    }
}