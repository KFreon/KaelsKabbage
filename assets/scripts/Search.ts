import Fuse, { FuseResult, FuseResultMatch } from "fuse.js";
interface SearchResult {
    isRender: boolean;
    href: string;
    title: string;
    tags: string[]
    text: string;
}

let resultsElement = document.getElementById('the-search-results-element');
let searchInputElement = document.getElementById('big-search-box') as HTMLInputElement;

let fullText: any[] | undefined = undefined;
let searcher: Fuse<SearchResult> | undefined = undefined;
let currentQueue: number | undefined = undefined;


// Load the full text search when we can
if (searcher === undefined) {
    fetch('search/FullText.json')
        .then(resp => {
            resp.json().then(json => fullText = json).catch(e => console.error(e));
        })
        .catch(err => console.error(err))
}

const fuseOptions = {"keys": ['title', 'tags', 'text'], threshold: 0.05, ignoreLocation: true, includeMatches: true, minMatchCharLength: 3, includeScore: true};

function showBigSearch() {
    const bigSearchModal = document.getElementById("big-search-background");
    bigSearchModal?.classList.add('fade-in')
    bigSearchModal?.classList.remove('fade-out')

    const box = document.getElementById("big-search-box")
    box?.focus();
}

function hideBigSearch() {
    const search = document.getElementById('big-search-box') as HTMLInputElement;
    const small = document.getElementById('searchbox') as HTMLInputElement;
    if (search === undefined) {
        console.error('search element not found')
        return;
    }
    if (small === undefined) {
        console.error('searchbox not found')
    }
    search.value = "";
    small.value = "";

    const bigSearchModal = document.getElementById("big-search-background");
    bigSearchModal?.classList.remove('fade-in')
    bigSearchModal?.classList.add('fade-out')
}

function insideSearchTextbox(event: Event) {
    event.preventDefault();
    event.stopPropagation();
}

function queueSearch() {
    if (currentQueue) {
        // cancel existing timeout
        clearTimeout(currentQueue)
    }

    currentQueue = setTimeout(() => {
        clearTimeout(currentQueue)
        currentQueue = undefined;
        search()
    }, 300);
}

function search() {
    if (fullText === undefined || resultsElement === null || searchInputElement === null) {
        return;
    }

    // Clear
    while (resultsElement.firstChild) {
        resultsElement.removeChild(resultsElement.lastChild!);
    }

    if (searchInputElement.value.length < 2) {
        toggleResults(false);
        return;
    }

    if (searcher === undefined) {
        searcher = new Fuse(fullText, fuseOptions)
    }

    const query = searchInputElement.value;
    const searchResults = searcher.search(query);
    renderResults(searchResults);
}

function renderResults(searchResults: FuseResult<SearchResult>[]) {
    if (resultsElement === null) return;
    
    if (!searchResults.length) {
        toggleResults(false);
        return;
    }

    toggleResults(true);

    // Only show the ten first results
    searchResults.slice(0, 4).forEach(function(result) {
        
        const item = result.item;
        const textMatches = result.matches

        const node = document.createElement("li");
        node.classList.add('search-result');

        if (item.isRender) {
            node.classList.add('is-render');
        }

        const link = document.createElement('a');
        link.href = item.href;
        link.classList.add('search-element')

        // Get title highlights, tags, etc
        const titleHighlights = getHighlightedText('title', textMatches, item.title)
        const titleNodes = generateHighlightedNodes('title', titleHighlights)

        const tagsHighlights = getHighlightedText('tags', textMatches, item.tags)
        const tagNodes = generateHighlightedNodes('tags', tagsHighlights)

        const textHighlights = getHighlightedText('text', textMatches, item.text)
        const textNodes = generateHighlightedNodes('text', textHighlights)

        titleNodes?.forEach(n => link.appendChild(n))
        textNodes?.forEach(n => link.appendChild(n))
        tagNodes?.forEach(n => link.appendChild(n))

        node.appendChild(link);
        resultsElement.appendChild(node);
    });
}

function generateHighlightedNodes(field: string, highlightedText: ReturnType<typeof getHighlightedText>) {
    // highlighedText [{key, parts: [{type, text}]}]
    // multiple is only tags really
    const elements = highlightedText?.filter(item => item !== undefined)
    .map(item => {
        const outer = document.createElement('p')
        outer.classList.add('search-element-' + field)

        const parts = item.parts.map(p => {
            const element = document.createElement('span')
            element.textContent = p.text
    
            if (p.type === 'highlighted') {
                element.classList.add('highlighted')
            }
            return element;
        })
        parts.forEach(p => outer.appendChild(p))
        return outer;
    })
    return elements
}

function getHighlightedText(field: string, textMatches: readonly FuseResultMatch[] | undefined, original: string | string[]) {
    const test = textMatches
    ?.filter(match => match.key === field)
    .map(match => {
        let index = 0;
        let parts: {type: 'normal' | 'highlighted', text: string}[] = [];
        const value = match.value ?? ""

        match.indices.slice(0, 5).forEach(range => {
            let normal = value.substring(index, range[0]);
            const matchText = value.substring(range[0], range[1] + 1);
            
            parts.push({type: 'normal', text: normal})
            parts.push({type: 'highlighted', text: matchText})
            index = +range[1] + 1;
        })
        let remaining = value.substring(index)
        parts.push({type: 'normal', text: remaining })

        // Truncate long text
        if (field === 'text') {
            parts = parts.map((p, i) => {
                if (p.type === 'normal' && p.text.length > 50) {
                    if (i === 0) {
                        // first
                        return {type: 'normal', text: '...' + p.text.substring(p.text.length - 50)}
                    }else if (i === parts.length - 1) {
                        // last
                        return {type: 'normal', text: p.text.substring(0, 50) + '...'}
                    } else {
                        return {type: 'normal', text: p.text.substring(0, 30) + '...' + p.text.substring(p.text.length - 30)}
                    }
                }
                return p;
            })
        }

        return { parts, key: match.key }
    })

    if (test && test.length > 0) return test;

    if (original === undefined) {
        return undefined;
    }

    if (original instanceof Array) {
        return original.map(o => ({
           key: field,
           parts: [{type: 'normal', text: o}] // tags aren't long 
        }))
    }
    else {
        return [{key: field, parts: [{type: 'normal', text: original.substring(0, 20) + '...'}]}];
    }
}

function toggleResults(isVisible: boolean) {
    if (isVisible) {
        resultsElement?.classList.add("visible")
    } else {
        resultsElement?.classList.remove("visible")        
    }
}