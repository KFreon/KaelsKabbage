// Original from somewhere I can't remember, sorry :(
// Also help from here: https://bart.degoe.de/searching-your-hugo-site-with-lunr/

var lunrIndex,
    $results,
    pagesIndex;

// Initialize lunrjs using our generated index file
function initLunr() {
    // First retrieve the index file
    $.getJSON("/js/PagesIndex.json", function(index) {
            pagesIndex = index;

            // Set up lunrjs by declaring the fields we use
            // Also provide their boost level for the ranking
            lunrIndex = lunr(function() {
                this.field("title", {
                    boost: 10
                });
                this.field("tags", {
                    boost: 5
                });
                //this.field("content");

                // ref is the result item identifier (I chose the page URL)
                this.ref("href");

                pagesIndex.forEach(page => {
                    this.add(page);
                }, this);
            });
        });
}

// Nothing crazy here, just hook up a listener on the input field
function initUI() {
    $results = $("#results");
    $results.empty();
    $("#searchbox").keyup(function() {
        $results.empty();

        // Only trigger a search when 2 chars. at least have been provided
        var query = $(this).val();
        if (query.length < 2) {
            toggleResults(false);
            return;
        }

        var results = search(query);

        renderResults(results);
    });

    $("#searchbox").blur(function() {
        if (document.documentElement.clientWidth < 960) {   // CSS mobile breakpoint
            toggleResults(false); 
        }
    });

    $("#searchbox").focus(function() {
        if ($results[0].children.length > 0) {
            toggleResults(true);
        }
    });
}

/**
 * Trigger a search in lunr and transform the result
 *
 * @param  {String} query
 * @return {Array}  results
 */
function search(query) {
    // Find the item in our index corresponding to the lunr one to have more info
    // Lunr result: 
    //  {ref: "/section/page1", score: 0.2725657778206127}
    // Our result:
    //  {title:"Page1", href:"/section/page1", ...}
    var lunrquery = lunrIndex.query(function(q) {
        // look for an exact match and give that a massive positive boost
        q.term(query, { usePipeline: true, boost: 100 });
        // prefix matches should not use stemming, and lower positive boost
        q.term(query, { usePipeline: false, boost: 10, wildcard: lunr.Query.wildcard.TRAILING });
      });

    var mapped = lunrquery.map(function(result) {
        return pagesIndex.filter(function(page) {
            return page.href === result.ref;
        })[0];
    });
    return mapped;
}

/**
 * Display the 10 first results
 *
 * @param  {Array} results to display
 */
function renderResults(results) {
    if (!results.length) {
        toggleResults(false);
        return;
    }

    toggleResults(true);

    // Only show the ten first results
    results.slice(0, 10).forEach(function(result) {
        var $result = $("<li class='search-result'>");
        $result.append($("<a>", {
            href: result.href,
            text: result.title
        }));
        $results.append($result);
    });
}

function toggleResults(isVisible) {
    if (isVisible) {
        $results.addClass("visible")
    } else {
        $results.removeClass("visible")        
    }
}

// Let's get started
initLunr();

$(document).ready(function() {
    initUI();
});