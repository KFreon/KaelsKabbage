---
title: "Blender Renders"
type: "renders"
---

I do like a bit of Blender, and I got some wind in my sails after doing the obligatory [BlenderGuru Donut tutorial](https://www.youtube.com/watch?v=TPrnSACiTJ4), so here's some of them.  
All tools, textures, and tutorials used are listed below.    

<!--more-->  

<details class="render-resources">
  <summary>Resources (sites, tutorials, tools, etc)</summary>
  <h2>Sites</h2>

  - [Poligon](https://www.poliigon.com/)  
  - [HDRIHaven](https://hdrihaven.com/)  
  - [Quixel](https://quixel.com/megascans/home/)
  - Models: [Mixamo](https://www.mixamo.com/)   

  <h2>Tools</h2>

  - [Blender](https://www.blender.org/) - Makes all this possible  
  - [Paint.NET](https://www.getpaint.net/) - Easy quick image editor  
  - [GIMP](https://www.gimp.org/) - xplat alternative  
  - [JSDisplacement](https://windmillart.net/?p=jsplacement) - Greeble generator  

  <h2>Tutorials</h2>

  - [The infamous doughnut](https://www.youtube.com/watch?v=TPrnSACiTJ4) - You gotta do it!  
  - [Anvil](https://www.youtube.com/watch?v=yi87Dap_WOc) - Modelling  
  - [Sword](https://www.youtube.com/watch?v=zHv4VDoCwYc) - Eevee
  - [Borg Cube](https://www.blendernation.com/2020/04/08/create-a-borg-cube-using-displacements/)  - Cool displacements  
</details>

<p class="render-toggle-buttons">
  <button id="render-list-button" class="selected" onclick="toggleRenderDisplay()">
    List
  </button>
  <button id="render-tiles-button" onclick="toggleRenderDisplay()">
    Tiles
  </button>
</p>

<div class="render-images" id="render-images">
{{< render_img description="Added a true Australian treat. Lamington :)" rendertime="~30 seconds" samples="20" >}}
{{% header_link "Doonut" %}}
{{< image path="img/Donut_WithMyMarble" alt="Obligatory Donut Render" nospacer="true" >}}
{{< /render_img >}}  

{{< render_img description="Donut gettin spun into scene like a champ" rendertime="~2 minutes" samples="20" frames="60" >}}
{{% header_link "Moovin Doonut" %}}
{{% video path="img/Donut_WithMyMarble" width="1920" height="1080" alt="I can almost hear it" %}}  
{{< /render_img >}}

{{< render_img description="I like floating rocks ¯\_(ツ)_/¯" rendertime="60 seconds" samples="20" >}}
{{% header_link "Floating Rocks" %}}
{{< image path="img/FloatingRocks" alt="Random floating rock" nospacer="true" >}}
{{< /render_img >}}  

{{< render_img description="I wanted to try making water with mist in the distance. I like it." rendertime="30 seconds" samples="30" >}}
{{% header_link "Ocean View" %}}
{{< image path="img/Ocean" alt="Bit of an ocean view" nospacer="true" >}}
{{< /render_img >}}  

{{< render_img description="This was going to be a bright grassy scene, but then I needed to hide the bad grass." rendertime="~1 hour" samples="100" >}}
{{% header_link "Spooky" %}}
{{< image path="img/Grass" alt="Spooky :) Ground turned out really good" nospacer="true" >}}
{{< /render_img >}}  

{{< render_img description="Olives are my favourite single food. Good inspiration." rendertime="25 seconds" samples="100" >}}
{{% header_link "Olives on a plate!" %}}
{{< image path="img/Olives" alt="Olives on a plate!" >}}
{{< /render_img >}}  

{{< render_img description="Finally fixed the olives. Note the differences mainly in the olives themselves, jar, and fork. I wasn't happy with the materials originally, and I want to show the difference now afterwards." rendertime="25 seconds" samples="100" >}}
{{% header_link "Fixed olives, highlighted differences" %}}
{{< image path="img/Olives_Difference" alt="Old olive render vs new bits" >}}
{{< /render_img >}}  

{{< render_img description="Coffee is the best thing, and sitting with one during the city buzz is very relaxing.| I was really happy with the bridge, but I realised that the DOF made it hard to see, so I did another one with less DOF :)" rendertime="15 minutes" samples="500" >}}
{{% header_link "Urban Takeaway Coffee" %}}
{{< image path="img/TakeawayCoffee" alt="Takeaway coffee up on a wooden table, so relaxing" >}}
{{< image path="img/TakeawayCoffee_Less_DOF" alt="Less DOF, since I'm pleased with the bridge :)" width="500px" >}}
{{< /render_img >}}  

{{< render_img description="My go at an enchanting table. Not really happy with it, but sometimes you're just done, you know?" rendertime="15 minutes" samples="500" >}}
{{% header_link "Enchanting table" %}}
{{< image path="img/EnchantingTable" alt="Enchanting table with some fun knick knacks" >}}
{{< /render_img >}}

{{< render_img description="The CG Fasttrack tutorial sword. I ended up using different assets or making some out of stubborness. The final result suffers for it, but I learned many new things." rendertime="5 seconds" samples="64" engine="Eevee" >}}
{{% header_link "Sword from CG Fasttrack" %}}
{{< image path="img/Sword" alt="Sword with cinematic compositing" >}}
{{< /render_img >}}

{{< render_img description="I almost like this one better. Still, compositing can add much more flair than I gave it credit for." rendertime="5 seconds" samples="64" engine="Eevee" >}}
{{% header_link "Sword WITHOUT cinematic compositing" %}}
{{< image path="img/Sword_Original" alt="Sword WITHOUT compositing" >}}
{{< /render_img >}}

{{< render_img description="I did another one from the other side so I could see all those cool runes :D" rendertime="5 seconds" samples="64" engine="Eevee" >}}
{{% header_link "Sword showing runes" %}}
{{< image path="img/SwordRunesView" alt="Sword showing runes" >}}
{{< /render_img >}}   

{{< render_img description="Eggster!" rendertime="1.5 minutes" samples="128" >}}
{{% header_link "Happy Easter!" %}}
{{< image path="img/Easter" alt="Easter Eggs" >}}
{{< /render_img >}}

{{< render_img description="Ancient areas are creepy places! I wanted to try my hand at animating. I need to do a tutorial..." rendertime="5 hours" samples="64" engine="Eevee" frames="1100" >}}
{{% header_link "Ancient Door" %}}
{{% video path="img/AncientDoor" width="1280" height="720" alt="Old dusty door animation" %}}  
{{< /render_img >}}

{{< render_img description="I got to practice some rigging, animations, and compositing :D It's from a show called Shadow Raiders. Some good sci-fi ideas in there." rendertime="2 hours" samples="400" engine="Cycles" frames="600" >}}
{{% header_link "The Beast Planet" %}}
{{% video path="img/ShadowRaiders" width="1280" height="720" alt="Shadow Raiders Beast Planet animation" %}}  
{{< /render_img >}}

{{< render_img description="Proving to myself I COULD do rocks with displacements, also the rope and grass." rendertime="11 minutes" samples="400" engine="Cycles" >}}
{{% header_link "Well and Bucket" %}}
{{< image path="img/CrackedWell" alt="Rocky well with a bucket and rope next to it" >}}
{{< /render_img >}}

{{< render_img description="The teapot was the hardest thing to mode. Feels like I did it the hardest way possible." rendertime="3.5 minutes" samples="300" engine="Cycles" >}}
{{% header_link "Tea and Biscuits" %}}
{{< image path="img/PoyasTea" alt="Tea and biscuits scene from a colleagues' photo" >}}
{{< /render_img >}}

{{< render_img description="Playing Halo MCC with a friend and figured it'd be easy to do that menu scene with all the bullets ¯\_(ツ)_/¯" rendertime="6 minutes" samples="800" engine="Cycles" >}}
{{% header_link "Bullets" %}}
{{< image path="img/Bullets" alt="Bullets scattered everywhere on a table" >}}
{{< /render_img >}}

{{< render_img description="Wanted to make a shattered world for a while. I'm not happy with this, but it's a start for next time. That volume tanked the rendertime too..." rendertime="20.5 minutes" samples="400" engine="Cycles" >}}
{{% header_link "Shattered World" %}}
{{< image path="img/ShatteredPlanet" alt="Shattered World with dust cloud" >}}
{{< /render_img >}}

{{< render_img description="I got a decent way through making this without a tutorial, so I'm pleased. The JSDisplacement greebles and AO emmision methods are really cool." rendertime="4.5 minutes" samples="400" engine="Cycles" >}}
{{% header_link "Borg Cube" %}}
{{< image path="img/BorgCube" alt="Borg Cube - you will be assimilated" >}}
{{< /render_img >}}

{{< render_img description="Playing with water and cloth sims" rendertime="12 hours" samples="400" engine="Cycles" frames="250" >}}
{{% header_link "University Water" %}}
{{% video path="img/WaterAndCloth" width="1280" height="720" alt="Water and cloth sims take ages" %}}  
{{< /render_img >}}

{{< render_img description="I love Control (Remedy game), and I wanted to play with boids" rendertime="13 hours" samples="300" engine="Cycles" frames="500" >}}
{{% header_link "SCP TV" %}}
{{% video path="img/ControlTV" width="1280" height="720" alt="Boids make this pretty nice, but they're hard to control" %}}  
{{< /render_img >}}

{{< render_img description="Found this in the old boxes. My first foray into Blender, took months. Then about 5 hours to make it way better. Relay model isn't mine" credits="Relay Model: SporeAltair@https://sketchfab.com/models/dbc16b9795234c6fa6c0a6ca8eaa4f00" rendertime="7 hours" samples="100" engine="Cycles" frames="1510" >}}
{{% header_link "Mass Relay" %}}
{{% video path="img/Relay" width="1280" height="720" alt="Asteroid fields, although unrealistic, feel awesome. As does space magic transportation." %}}  
{{< /render_img >}}








</div>