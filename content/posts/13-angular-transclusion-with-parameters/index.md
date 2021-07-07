---
title: "Angular Transclusion With Parameters"
date: 2019-03-01T15:51:30+10:00
draft: false
type: "post"
slug: angular-transclusion-with-parameters
tags: ["angular"]
---

I recently came across a scenario where I had several components with the same structure in them, so I figured I'd extract it out into it's own component.   
Simple right? Everyday scenario?  
Were it so easy...  

<!--more-->  

**TL;DR**  
*Problem*: Can't do transclusion and pass properties to content.  
*Solution*: Use `ng-template` with `ng-container *ngTemplateOutlet` with a `context: { $implicit: variable-name }` property.  

# The problem  
{{< split >}}
{{% splitLeft title="Html" %}}
``` html
<div>
    <h1>Super cool page title</h1>
    <p>You're going to love this demo</p>

    <div *ngFor="let holiday of holidays">
        <div *ngFor="let location of holiday.locations">
            <strong>{{ location.name }}</strong>
            <p>{{ location.picture }}</p>
        </div>
    </div>
</div>
```  
{{% /splitLeft %}}
{{% splitRight title="Typescript" %}}
``` ts
import { Component, OnInit } from '@angular/core';

export interface Location {
    name: string;
    picture: string;
}

export interface Holiday {
    locations: Location[];
}

@Component({
    selector: 'app-original',
    templateUrl: './original.component.html',
    styleUrls: ['./original.component.scss']
})
export class OriginalComponent implements OnInit {
    holidays: Holiday[] = [];

    constructor() { }

    ngOnInit() {
        this.holidays = []; // Initialisation removed for brevity
    }
}
```
{{% /splitRight %}}
{{< /split >}}  

I wanted to extract the two `*ngFor` divs into their own component for reuse in another part of the codebase.   
I wanted to be able to do something like:   
``` html
<div>
    <h1>Super cool page title</h1>
    <p>You're going to love this demo</p>

    <EXTRACTED-COMPONENT [data]="holidays">
        <strong>{{ location.name }}</strong>
        <p>{{ location.picture }}</p>
    </EXTRACTED-COMPONENT>
</div>
```  

I searched around the internet [and](https://stackoverflow.com/questions/42978082/what-is-let-in-angular-2-templates ) found [several](https://blog.angular-university.io/angular-ng-content/ ) relevant [posts](https://medium.com/claritydesignsystem/ng-content-the-hidden-docs-96a29d70d11b) and [articles](https://stackoverflow.com/questions/51807192/passing-for-variable-to-ng-content) (links also at the bottom of the article), but none really explained how it worked or how to make it work properly.  

# Transclusion  
Transclusion was the first stop. This is where an element indicates where it's child content will be rendered without explicitly knowing what the content is.  
Angular provides the `ng-content` component for this purpose.  

##### transclusion.component.html
``` html
<div> 
    <p>This is a title</p>
    <ng-content></ng-content>
</div>
```   

Such a component would be used like this:
``` html
<transclusion>
    <p>This is some content</p>
</transclusion>
```  

And you'd get:   

``` go
This is a title  
This is some content   <--- Pulled through the ng-content
```  

However I'm looking to have the transclusion **BUT** with some context from structural directives like `*ngFor`. I figured you'd be able to pass some context to this content, but unfortunately `ng-content` doesn't allow dynamic content (docs says it's performed at compile time), so I needed another solution.  

I talked with [Tristan Menzel](https://github.com/tristanmenzel) about how to approach this, and he pointed me towards structural directives and templates.   

<br/>  

# Templates  
`ng-template` provides a way of specifying dynamic transclusion content that can be modified/populated at runtime.  

Templates can be rendered inside a `ng-container` via the `*ngTemplateOutput` directive.  

``` html
<div>
    <ng-template let-location>
        <p>{{ location.name }}</p>
    </ng-template>
</div>
```

``` html
<ng-container *ngTemplateOutlet="template; context: { $implicit: location }"></ng-container>
```    
The above code indicates that the `ng-container` should render the template called *template* providing it with the implicit knowledge of a variable called *location*, so the template itself can function. How does this context work?  
It'll be easier to just go through the solution as a whole.    

# My Solution  
I had a hard time putting this succinctly, so let's start with the finished source.  

## Extracted component   

{{< split >}}
{{% splitLeft title="Html" %}}
``` html
<div *ngFor="let holiday of holidays">
    <div *ngFor="let location of holiday.locations">
        <ng-container *ngTemplateOutlet="template; context: { $implicit: location }"/>
    </div>
</div>
```
{{% /splitLeft %}}
{{% splitRight title="Typescript" %}}
``` ts
import { Component, OnInit, Input, ContentChild, TemplateRef } from '@angular/core';
import { Holiday } from '../original/original.component';

@Component({
    selector: 'app-extracted',
    templateUrl: './extracted.component.html',
    styleUrls: ['./extracted.component.scss']
})
export class ExtractedComponent implements OnInit {

    @Input() holidays: Holiday[];

    // Searches the tree for an element of type TemplateRef (ng-template) 
    // and sets this property
    @ContentChild(TemplateRef) template: TemplateRef<any>;

    constructor() { }

    ngOnInit() {
    }
}
```
{{% /splitRight %}}
{{% /split %}}

<br/>  

## Refactored component  

{{< split >}}
{{% splitLeft title="Html" %}}
``` html
<div>
    <h1>Super cool page title</h1>
    <p>You're going to love this demo</p>

    <app-extracted [holidays]="holidays">
        <ng-template let-location>
            <strong>{{ location.name }}</strong>
            <p>{{ location.picture }}</p>
        </ng-template>
    </app-extracted>
</div>
```
{{% /splitLeft %}}
{{% splitRight title="Typescript" %}}
``` ts
import { Component, OnInit } from '@angular/core';

export interface Location {
    name: string;
    picture: string;
}

export interface Holiday {
    locations: Location[];
}

@Component({
    selector: 'app-refactored',
    templateUrl: './refactored.component.html',
    styleUrls: ['./refactored.component.scss']
})
export class RefactoredComponent implements OnInit {
    holidays: Holiday[] = [];

    constructor() { }

    ngOnInit() {
        this.holidays = []; // Initialisation removed for brevity
    }
}

```
{{% /splitRight %}}
{{< /split >}}  

**EDIT:** Note that for reduced black box magic, you can specify the template directly instead of with `@ContentChild`.  
See below. `@ContentChild` replaced in the Typescript with `@Input()` and the html altered slightly to give the template an id with `#template` and the `app-extracted` component takes the template as a variable directly.  

``` html
<app-extracted [holidays]="holidays" [template]="template">
    <ng-template let-location #template>
        <strong>{{ location.name }}</strong>
        <p>{{ location.picture }}</p>
    </ng-template>
</app-extracted>
```

# Steps I took  
1. Pull those `*ngFor` divs out into a new component  
2. Provide the new component with the data previously given to the outer `*ngFor`
3. Wrap the content of the inner `*ngFor` in a `ng-template` instead
  * We need to provide the `location` variable to this as it's undefined right now
4. Provide context via the `let-location` directive 
  * *let-variablename* creates an implicit variable called *variablename* for the code block. This means that the variable is expected to be defined in the context provided to the template.  
5. The extracted component requires a template outlet container, somewhere to render the `ng-template` we just defined.  
  * We also need to setup that implicit variable for the `ng-template` context.

## Some detail 

* `*ng-container` renders the `ng-template` from the parent component.  
* In this example, the `ng-template` is discovered by the `ng-container` via the `@ContentChild(TemplateRef)` decorator.  
  * Searches the tree for the first occurrance of the indicated type (TemplateRef, which here is a `ng-template`)  
* The template gets the inner variable via the `context: { $implicit: location}` part of the `ng-container *ngTemplateOutput` statement.  
  * Coupled with the `ng-template let-location` statement, this creates an implicit context containing the variable **location** as if it was declared.   
  * There's no intellisense provided for the `let-location` or any use of the **location** variable afterwards.

### Take home points
* *let-variable* indicates to `ng-template` an implicit variable it can expect  
* Context can be provided to `ng-container` by expanding on the `*ngTemplateOutlet` property  
* `@ContentChild` can be used to find the template for you or you can provide it as a standard angular input variable (but looks messier)  
* The combination of all this allows transclusion with passed variables


<br/>  

# Building blocks:  
https://stackoverflow.com/questions/42978082/what-is-let-in-angular-2-templates  
https://blog.angular-university.io/angular-ng-content/   
https://medium.com/claritydesignsystem/ng-content-the-hidden-docs-96a29d70d11b  
https://stackoverflow.com/questions/51807192/passing-for-variable-to-ng-content  