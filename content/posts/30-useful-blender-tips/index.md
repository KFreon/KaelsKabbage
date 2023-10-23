---
title: "Some Useful Blender Tips"
date: 2022-01-27T12:11:35+10:00
type: "post"
slug: "useful-blender-tips"
tags: ["blender"]
---

Some Blender tips and tricks that I've built up over my short time, so saving them here so I don't forget them :D   
This will keep being updated as I think of them.  

<!--more-->  

# Circular Array  
This used to be a weird, complicated thing about parenting empties with offset origins, etc.  
Now, we just get a circle with verticies where you want them, then use Geometry nodes to instance whatever you want onto it.  
So EZ now!

# Edit View: Local coords  
`shift + numpad 1,7,3, etc` aligns view to object LOCAL coords instead of global!!!!  
This is super useful if you've rotated the object away from norms.  
e.g. You have a cube, you rotate it to align with something else, but then you want to do some modelling on a face of it but pressing the 1,3,7 combos now don't line up.
`shift + 1,3,7` will align to local coords so you can!  

# Quick Geometry Nodes Displace setup  
The below nodes plug into the Offset socket of the "Set Position" node.  
{{< image path="img/Geonodes_displace" alt="Basic displacement geometry nodes setup" >}}  

# Geometry nodes weirdness  
- Collection Info node sometimes needs "Realise Instances" node
  - This is usually when you need to modify the instances e.g. "Set Position", scale, etc
- Collection Info node sometimes need to tick "Separate Children" and "Reset Children" and "Pick Instance" with Random or something plugged in
  - This is usually for instancing items from the collection, instead of the whole collection, but I've had weird behaviour around this before. i.e. needed to tick it when I didn't think I should.  
- MAKE SURE SCALE IS APPLIED!!!  
- If you don't want a field, get "Object Info" and plug "Location" into a socket like Index or ID  
  - Especially useful for node groups that take scalar values e.g. seed
  - Issue is that if you plug "Random Value" into Seed so you get a random value per displace, it does it as a field (Random output is a field), which means that EACH POINT gets a different seed
  - Instead, turn the Random output into a scalar with this trick  
{{< image path="img/Scalar-Field-Conversion" alt="Weird trick to get a scalar value out of a field" >}}  



# Quick procedural wood setup  
No example, but just a noise texture with some distortion stretched on one axis, include bump map, usually based on the same noise texture.  


# Geometry nodes: Look at  
{{< image path="img/LookAt" alt="Have all instances look into the middle" >}}