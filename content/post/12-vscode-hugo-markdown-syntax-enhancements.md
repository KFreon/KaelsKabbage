---
title: "VSCode Extension: Hugo syntax highlighting for Markdown"
date: 2019-02-25T10:45:38+10:00
slug: "vscode-hugo-markdown-syntax-enhancements"
draft: false
---

**EDIT:** After this, I realised my shortcodes were a bit annoying to type, so I made some snippets for them (at the end of the page)

Some time ago, just after the first post on this blog, I started using [Hugo Shortcodes](https://gohugo.io/content-management/shortcodes/).  
The theme I was (still am) using didn't have great support for some of the markdown stylings that I wanted, and instead of fiddling with that, I figured I'd create my own with custom styles and Hugo Shortcodes.  

This ended up working fine, things displayed how I wanted them and all was right, except when I went back to posts and had a look.  
{{% image path="/img/HugoShortcodes/HugoShortcodes_NoExtension" alt="Hugo Shortcodes with no syntax highlighting" %}}  

Bit hard to see and visually differentiate all the elements.   
After installing my extension:   
{{% image path="/img/HugoShortcodes/HugoShortcodes_WithExtension" alt="Syntax highlighting enabled via TextMate Markdown extension" %}}

At the time, it wasn't possible to have an extension that modified VSCode's markdown syntax (as far as I could tell), so I was forced to dive into the {{% inline "markdown.tmLanguage" %}} file directly and change stuff. It wasn't ideal and I had to do it every time VSCode updated.  

Recently, the markdown syntax became extensible through the [TextMate](https://macromates.com/manual/en/language_grammars) language extensions and I decided to finally put this to bed.  
I started reading the [VSCode Extensions](https://code.visualstudio.com/api) documentation and found it lacking the little bits of info I needed to contextualise what I was doing.  
Long story short, I trawled through posts and discovered [this post](https://www.marcobeltempo.com/open-source/inject-frontmatter-syntax-markdown/) and [this repo](https://github.com/marcobeltempo/vscode-fenced-toml) which gave me some confidence and clarification on what I was doing.  

**TLDR** of my learnings are:  

* Regex is hard.  
* Really do follow the VSCode Extensions instructions especially regarding Yeoman. Very handy, very simple.   
  * Note that I didn't base mine off any other tmLanguage since it wasn't adding a new language, just extending Markdown.   
* {{% inline "injectTo" %}} in the **grammars** section of **package.json** and {{% inline "injectionSelector" %}} in the main area of the **tmLanguage** file are key.  
* Some research for styling will probably be necessary.  
* Regex is hard. (I mean really...)

``` regex
(\w+)\s([a-zA-Z]+)=\"(\S+)\"\s*(?:([a-zA-Z]+)=\"([^\"]+)\")*
```  
<br>  

## Getting Started
After doing a {{% inline "yo code" %}} and following the instructions, you end up with a fairly blank slate.  First stop was {{% inline "package.json" %}} to specify what the extension was going to do. Most of it should be filled out sufficiently (although you'll need to add a **publisher** field in order to package or publish). The area to focus on is the {{% inline "languages" %}} and {{% inline "grammars" %}}.    
  <br>

## Code Dive
### package.json
**Languages** should be filled out and isn't really relevant to this situation as it relates to adding support for a currently unsupported language.  
{{% image path="/img/HugoShortcodes/Hugo_Languages" alt="Languages section in package.json" %}}  

**Grammars** contains information about how the syntax highlighting should behave.  
{{% image path="/img/HugoShortcodes/Hugo_Grammars" alt="Grammars section in package.json" %}}  

It's name is the language name from the id above (not relevant here either), scope name is something to do with styles and needs to match the {{% inline "<name>.tmLanguage.json" %}} file ([see this post](https://www.apeth.com/nonblog/stories/textmatebundle.html) for more info on scopes and themes)  

The most important part for me here was {{% inline "injectTo" %}} as it told VSCode that I wanted these grammar rules to be applied to an existing set of rules defined somewhere else. In my case, Markdown ({{% inline "text.html.markdown" %}}). I believe these come from TextMate Grammars as well.  


### *name*.tmLanguage.json
Next, the tmLanguage file. It should be scaffolded in some fashion, but I'll go through the bits as I understand them.  
The first few should be self explanatory and not relevant ($schema, name), then we're gonna add one called {{% inline "injectionSelector" %}} which tells VSCode/TextMate where to inject this set of things. I assume both are needed (injectTo and injectionSelector), but I haven't confirmed.  

**Patterns** and optionally **Repository** are the next interesting sections.  
Patterns is the set of rules and Repository is a way to make things more readable. It's like refactoring large functions into smaller ones. Repository stores named rules that can be based on each other, reference each other, etc. They're then included in the patterns section.  

#### Example
```json
"patterns": [
    {
        "include": "#strings"
    }
],
"repository": [{
    "strings": {
        "begin": "--",
        "beginCaptures": {
            "0": {
                "name": "punctuation.section.embedded.end.hugo"
            }
        },
        "end": "--",
        "patterns": [{
            "match": "([a-zA-Z]+)",
            "name": "support.function.hugo",
            "captures": {
                "1": {
                    "name": "string.other.link.title.markdown"
                }
            }
        }]
    }
}]
```  
From this, we can see a rule called {{% inline "strings" %}} is in the repository, and it's included in the patterns section. You can have rules in the repository that aren't included in the patterns, e.g. for breaking up complicated rules.  

The {{% inline "strings" %}} rule has *begin* and *end* entries which allows separate styling of the start/end markers (via the beginCaptures/endCaptures names), and a *patterns* section for regex matching of the main rules. In this case, it styles the first capture group as a markdown title (colours only it seems)  

Multiple patterns can be included for matching syntactically different, yet logically the same things (e.g. Hugo shortcodes can have different shapes, but are all still shortcodes). 
Multiple capture groups are also possible to style different parts of the syntax expression e.g. formatting a keyword differently to containing text.  

In the end, I came up with some syntax highlighting rules for Hugo Shortcodes, specifically my shortcodes and style thereof, so there's no guarantee that it'll work for anyone elses, but perhaps it'll help someone out.  

[Extension repository](https://dev.azure.com/kaellarkin/_git/Hugo-Shortcode-Syntax-Highlighting) (not currently published)  

EDIT  
<details>
    <summary>Snippets</summary>  

    "HugoShortcode Image": {
        "prefix": "himage",
        "body": [
            "{{%/* image path=\"img/${1:folder}\" alt=\"${2:alt-text}\" */%}}"
        ],
        "description": "Image ShortCode"
    },

    "HugoShortcode Inline": {
        "prefix": "hinline",
        "body": [
            "{{%/* inline \"${1:text}\" */%}}"
        ],
        "description": "Inline ShortCode"
    }
</details>

<details>
<summary>Final syntax highlighting (tmLanguage)</summary>
```json
{
	"$schema": "https://raw.githubusercontent.com/martinring/tmlanguage/master/tmlanguage.json",
	"name": "markdown",
    "injectionSelector": "L:text.html.markdown",
	"patterns": [
		{
			"include": "#image"
		}
	],
	"repository": {
		"image": {
            "begin": "{{%/*|{{<",
            "beginCaptures": {
                "0": {
                    "name": "punctuation.section.embedded.begin.hugo"
                }
            },
            "end": ">}}*/%}}",
            "endCaptures": {
                "0": {
                    "name": "punctuation.section.embedded.end.hugo"
                }
            },
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
            {
                "match": "([a-zA-Z]+)\\s\\\"([^\"]+)\\\"",
                "name": "support.function.hugo.shorter",
                "captures": {
                    "1": {
                        "name": "string.other.link.title.markdown"
                    },
                    "2": {
                        "name": "markup.italic.markdown"
                    }
                }
            }]
		}
	},
	"scopeName": "text.markdown.hugo"
}
```
</details>