---
title: "Angular Transclusion With Parameters"
date: 2019-03-01T15:51:30+10:00
draft: true
slug: angular-transclusion-with-parameters
---

I recently came across a scenario where I had several components with the same structure in them, so I figured I'd extract it out into it's own component.   
Simple right? Everyday scenario?  
This one was more complicated than expected.   

# Transclusion  
Transclusion came to mind first. The idea of an element indicating where it's child content will be rendered.  
Angular provides the {{% inline "ng-content" %}} component for this purpose.  

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
```
This is a title  
This is some content
```  

However I'm looking to have the transclusion **BUT** with some context from structural directives like {{% inline "*ngFor" %}}.  
{{% inline "ng-content" %}} doesn't allow dynamic content (docs says it's performed at compile time), so I needed another solution.  
I talked with [Tristan Menzel](https://github.com/tristanmenzel) about how to approach this, and he pointed me towards structural directives and templates.   

<br/>  


# Templates  
{{% inline "ng-template" %}} provides a way of specifying a template to use given some information. Where {{% inline "ng-content" %}} provides a placeholder, {{% inline "ng-template" %}} indicates that it's contents are to be used as a template for some other area.  

## Containers  
Templates can be rendered inside a {{% inline "ng-container" %}} via the {{% inline "*ngTemplateOutput" %}} directive.  

``` html
<div>
    <ng-template>
        <p>Property Name </p>
    </ng-template>
</div>
```

``` html
<ng-container *ngTemplateOutlet="template"></ng-container>
```  

This is all difficult to show without an example, so here's one I prepared earlier.  

<br/>

# Example  
I have a component that looks like this:  
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

Lets say then, that I have several components that do the same type of thing with the double {{% inline "*ngFor" %}} and that there's additional styling and functional complexity involved with that, so I wanted to extract it into its own component.  

I'd like to be able to do something like this:   
``` html
<div>
    <h1>Super cool page title</h1>
    <p>You're going to love this demo</p>

    <EXTACTED-COMPONENT [data]="holidays">
        <strong>{{ location.name }}</strong>
        <p>{{ location.picture }}</p>
    </EXTACTED-COMPONENT>
</div>
```  

Of course, the above doesn't work, so I'm going to use templates and containers to get things almost that good.  
The completed source is below.  

<br/>  

## Extracted:  
``` html
<div *ngFor="let holiday of holidays">
    <div *ngFor="let location of holiday.locations">
        <ng-container *ngTemplateOutlet="template; context: { $implicit: location }"></ng-container>
    </div>
</div>
```

Backend:  
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
    @ContentChild(TemplateRef) template: TemplateRef<any>;

    constructor() { }

    ngOnInit() {
    }

}
```  

<br/>  

## Refactored  
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

Backend:  
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


Building blocks:  
https://stackoverflow.com/questions/42978082/what-is-let-in-angular-2-templates  
https://blog.angular-university.io/angular-ng-content/   
https://medium.com/claritydesignsystem/ng-content-the-hidden-docs-96a29d70d11b  
https://stackoverflow.com/questions/51807192/passing-for-variable-to-ng-content  