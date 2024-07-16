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
        // searcher = new Fuse(fullText, {"keys": ['title', 'tags', 'text'], "includeMatches": true})
        searcher = new Fuse(fullText, {"keys": ['title'], "includeMatches": true})
    }
    const query = searchInputElement.value;

    const searchResults = searcher.search(query);
    renderResults(query, searchResults);
}

function renderResults(query, searchResults) {
    if (!searchResults.length) {
        toggleResults(false);
        return;
    }

    toggleResults(true);

    // Only show the ten first results
    searchResults.slice(0, 4).forEach(function(result) {
        const item = result.item;
        const textMatches = result.matches; // []
        const title = item.title;
        const matchingText = "";

        const node = document.createElement("li");
        node.classList.add('search-result');

        if (item.isRender) {
            node.classList.add('is-render');
        }

        const link = document.createElement('a');
        link.href = item.href;
        link.create

        const highlightInfo = getHighlightedText(title, query, textMatches)
        generateHighlightedNodes(link, highlightInfo)

        const matchingTextEl = document.createElement('span');
        matchingTextEl.textContent = matchingText;

        node.appendChild(link);
        node.appendChild(matchingTextEl);
        resultsElement.appendChild(node);
    });
}

function generateHighlightedNodes(node, highlightedText) {
    console.log()
    highlightedText.forEach(text => {
        const element = document.createElement('span')
        element.textContent = text.text

        if (text.type === 'highlighted') {
            element.classList.add('highlighted')
        }
        node.appendChild(element)
    })
}

function getHighlightedText(title, query, textMatches) {
    let parts = [];
    let index = 0;
    textMatches
        .flatMap(match => match.indices.map(i => ({ range: i, key: match.key, value: match.value })))
        .forEach(match => {
            const range = match.range
            // range [[start, len]]

            const normal = title.substring(index, range[0]);
            const matchText = title.substring(range[0], range[1] + 1);
            parts.push({type: 'normal', text: normal})
            parts.push({type: 'highlighted', text: matchText})

            index = +range[1] + 1; // + makes them numbers
    })
    parts.push({type: 'normal', text: title.substring(index)})

    return parts;
}

function toggleResults(isVisible) {
    if (isVisible) {
        resultsElement.classList.add("visible")
    } else {
        resultsElement.classList.remove("visible")        
    }
}