---
title: "ESLint custom rules"
date: 2025-05-14T09:54:00+10:00
type: "post"
slug: "eslint-custom-rules"
tags: ["eslint"]
---

I had a scenario where I had some locale date functions being used where we actually wanted our own date formatting functions.  
It caused the dates to display differently in Edge compared to everything else, which wasn't a problem, but wasn't right either.  
It got me wondering if we could use linting to warn/prevent use of these functions.  

> This is mostly for me so I don't forget.  
> There are a bunch of better blog posts linked here.  

<!--more-->  

# The scenario  
In more detail, we have some `formatDate` functions that we want to use instead of the builtin `date.toLocaleDateString()` (for example).  
I want to detect uses of `toLocaleDateString` and put a yellow squiggly with a message saying to use ours instead.  

# ESLint  
We have ESLint, so I went looking.  
Turns out to be easy, and hard at the same time.  

Create a new eslint plugin and add the rules thusly.  

```js
// /my-custom-eslint-rules/rules/no-builtin-date-functions.js
module.exports = {  
    meta: {  
        type: "problem",  
        docs: {  
            description: "Date functions can vary based on browser and OS. Use the custom functions like `formatDate` instead",  
        },  
        fixable: "code",  
        schema: [], // no options  
    },  
    create: function (context) {  
        const functions = ['toLocaleDateString', 'toDateString', 'toISOString', 'toLocaleString', 'toLocaleTimeString', 'toTimeString']  
        return {  
            MemberExpression(node){  
                // This currently just looks for the text.  
                // We could search for ONLY Date usages (instead of also moment usages), but there's a bunch of nodes found that are "undefined"  
                // I don't know what that really means right now, so not doing it.  
                // But for future reference: node?.object?.callee?.name  
                if(functions.includes(node.property.name)){  
                    context.report({  
                        node,  
                        message: "Date functions can vary based on browser and OS. Use the custom functions like `formatDate` instead"  
                    })  
                }  
            }  
        };  
    },  
};

// /my-custom-eslint-rules/package.json
{
    // The 'eslint-plugin-' prefix is required
  "name": "eslint-plugin-my-custom-rules",  
  "version": "1.0.0",  
  "main": "index.js"  
}

// index.js
const noBuiltinDateFunctions = require("./rules/no-builtin-date-functions");  
  
const plugin = {   
  rules: {   
    "no-builtin-date-functions": noBuiltinDateFunctions  
  }   
};  
module.exports = plugin;
```

Install the plugin with `npm install -D file:my-custom-rules`  

Then add the plugin to the eslint config.  
I'm still using `eslintrc.json`.  

```json
// eslintrc.json
{
  "plugins": [
  //...others,
  'my-custom-rules'
  ],
  "rules": {
    "my-custom-rules/no-builtin-date-functions": "warn"
  }
}
```

# How to figure out what to select?  
Figuring out what should be in the `create` method is hard, and there's a [site](https://explorer.eslint.org/) to help figure it out, but I just used chatgippity which did a good enough job of it.  
You can also do `console.log` and go to Output ==> Eslint in vscode to see that output.  

---  

Blog posts I got most of this from:  
- [Create a custom ESLint Plugin — In a Simple and easy way](https://sinhahariom1.medium.com/create-a-custom-eslint-plugin-in-a-simple-and-easy-way-055b3baa2b5e)  
- [How to easily define and configure your own ESLint rules without the hassle](https://medium.com/@avrahamhamu/how-to-easily-define-and-configure-your-own-eslint-rules-without-the-hassle-a15d1fb0134f)