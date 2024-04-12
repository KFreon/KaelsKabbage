---
title: "Hugo Syntax Extension: Revisited"
date: 2020-01-31T21:32:07+10:00
type: "post"
slug: "hugo-syntax-extension-revisited"
tags: ["vscode", "markdown"]
---

Hugo Shortcode Syntax Highlighter [VSCode extension](https://marketplace.visualstudio.com/items?itemName=kaellarkin.hugo-shortcode-syntax) is live!

More improvements in [April 2024](#april-2024-updates)

<!--more-->  

I recently got a new laptop and realised that I didn't have the [extension](/content/posts/12-vscode-hugo-markdown-syntax-enhancements/index.md) I'd half-heartedly made last year.  
When I did go back and get it, I realised it was pretty weak and flimsy in it's syntax highlighting.  
Most notably, I added a new shortcode with more parameters than two and the extension wouldn't highlight more than two...  

The long and short of it is that I was trying to do all the highlighting with a single regex, and that just doesn't work.  
Any number of parameters should ne work, and the highlighting is a bit nicer now.  

Positional parameters aren't really supported though :(  
I don't use them so I'm not fussed, but I'm also not sure how I'd detect the shortcode name compared to the positional parameters...  

The new area of the `.tmLanguages.json` is below.  

{{% splitter %}}
{{% split side=left title="Before" %}}
```json
"patterns": [{
    "match": "(\\w+)\\s([a-zA-Z]+)=\\\"(\\S+)\\\"\\s*(?:([a-zA-Z]+)=\\\"([^\"]+)\\\")*",
    "name": "support.function.hugo.main",
    "captures": {
        "1": {
            "name": "string.other.link.title.markdown"
        },
        "2": {
            "name": "markup.bold.markdown"
        },
        "3": {
            "name": "markup.italic.markdown"
        },
        "4": {
            "name": "markup.bold.markdown"
        },
        "5": {
            "name": "markup.italic.markdown"
        }
    }
},
```
{{% /split %}}
{{% split side=right title="After" %}}
```json
"patterns": [{
    "EXPLANATION": "Matches the shortcode name (at least). Matches single words, optionally with / at the start.",
    "match": "(/?\\w+)\\s",
    "name": "support.function.hugo.main",
    "captures": {
        "1": {
            "name": "string.other.link.title.markdown"
        }
    }
},
{
    "EXPLANATION": "Matches parameter name specifiers, e.g. title=, Matches any word ending with =",
    "match": "(\\w+=)",
    "name": "support.function.hugo.params",
    "captures": {
        "1": {
            "name": "markup.bold.markdown"
        }
    }
},
{
    "EXPLANATION": "Matches parameter values i.e. things in quotes",
    "match": "\"([^\"]+)\"",
    "name": "support.function.hugo.paraminfo",
    "captures": {
        "1": {
            "name": "markup.italic.markdown"
        }
    }
}
```
{{% /split %}}
{{< /splitter >}}  


# April 2024 updates  
I recently added support in v2 to suggest user created shortcodes, as well as F12 drilldown to the shortcode in question.  

I also made another very simple [related extension](https://marketplace.visualstudio.com/items?itemName=kaellarkin.hugo-tags-helper) for suggesting tags in frontmatter.  