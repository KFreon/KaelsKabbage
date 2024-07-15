let searchResults = [];
let resultsElement = document.getElementById('the-search-results-element');
let searchInputElement = document.getElementById('big-search-box');

let searcher = undefined;

function showBigSearch() {
    const bigSearchModal = document.getElementById("big-search-background");
    bigSearchModal.classList.add('fade-in')
    bigSearchModal.classList.remove('fade-out')

    const box = document.getElementById("big-search-box")
    box.focus();
}

function hideBigSearch() {
    const search = document.getElementById('big-search-box');
    const small = document.getElementById('searchbox');
    search.value = "";
    small.value = "";

    // const bigSearchModal = document.getElementById("big-search-background");
    // bigSearchModal.classList.remove('fade-in')
    // bigSearchModal.classList.add('fade-out')
}

function insideSearchTextbox(event) {
    event.preventDefault();
    event.stopPropagation();
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

    if (searcher === undefined) {
        searcher = new Fuse(fullText, {"keys": ['title', 'tags', 'text'], "includeMatches": true})
    }
    const query = searchInputElement.value;

    searchResults = searcher.search(query);
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
        const text = result.title;
        const matchingText = "";
        console.log(result)

        const node = document.createElement("li");
        node.classList.add('search-result');

        if (result.isRender) {
            node.classList.add('is-render');
        }

        const link = document.createElement('a');
        link.href = result.href;
        link.textContent = text;

        const matchingTextEl = document.createElement('span');
        matchingTextEl.textContent = matchingText;

        node.appendChild(link);
        node.appendChild(matchingTextEl);
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