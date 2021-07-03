---
title: "Blender Renders"
type: "renders"
---

I do like a bit of Blender, and I got some wind in my sails after doing the obligatory [BlenderGuru Donut tutorial](https://www.youtube.com/watch?v=TPrnSACiTJ4), so here's some of them.  
All tools, textures, and tutorials used are listed below.    

<!--more-->  

<details class="render-resources">
  <summary>Resources (sites, tools, etc)</summary>
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
</details>

<p class="render-toggle-buttons">
  <button id="render-list-button" onclick="setRenderDisplay('list')">
    List
  </button>
  <button id="render-tiles-button" onclick="setRenderDisplay('tiles')">
    Tiles
  </button>
  <button id="render-carousel-button" class="selected" onclick="setRenderDisplay('carousel')">
    Carousel
  </button>
</p>

<div class="render-images slider" id="render-images">
<div class="slides" id='slides'>

{{< render_img
  description="Nice relaxing book for my sisters birthday!"
  renderdate="2021-7-03"
  rendertime="1 hour"
  frames="60"
  samples="200"
  >}}
{{% header_link "Relaxing Nighttime Writing" %}}
{{% next_link "Pirate Treasure" %}}
{{< video path="img/Book" alt="Book with candle, pen, and hot drink!" >}}
{{< /render_img >}}

{{< render_img
  description="Pirate Treasure for a Reddit contest"
  renderdate="2021-6-24"
  rendertime="80 seconds"
  samples="200"
  >}}
{{% previous_link "Relaxing Nighttime Writing" %}}
{{% header_link "Pirate Treasure" %}}
{{% next_link "Age of Empires Mill Perspective" %}}
{{< image path="img/PirateTreasure" alt="Pirates Treasure chest with gold, the typical pirate treasure scene :)" >}}
{{< /render_img >}}

{{< render_img
  description="A much more interesting angle with some extras"
  renderdate="2021-5-6"
  rendertime="4 minutes"
  samples="150" >}}
{{% previous_link "Pirate Treasure" %}}
{{% header_link "Age of Empires Mill Perspective" %}}
{{% next_link "Age of Empires Mill Comparison" %}}
{{< image path="img/AOEMill_Perspective" alt="Better perspective, characters, and doodads" >}}
{{< /render_img >}}

{{< render_img
  description="Watching some Age of Empires videos got me thinking|That mill looks pretty nice. Many experiments here, most successful."
  renderdate="2021-5-6"
  rendertime="4 minutes"
  samples="150" >}}
{{% previous_link "Age of Empires Mill Perspective" %}}
{{% header_link "Age of Empires Mill Comparison" %}}
{{% next_link "Blowing Desert" %}}
{{< image path="img/AOEMill_Comparison" alt="Comparison to reference, Age of Empires Mill" >}}
{{< /render_img >}}

{{< render_img
  description="I saw Pwnisher's car scene and for some reason, the moving volume got me."
  renderdate="2021-4-30"
  rendertime="2 hours"
  samples="200"
  credits=""
  engine="Cycles (with denoiser node)"
  frames="100"
  tutorialUrl=""
  tutorialText="" >}}
{{% previous_link "Age of Empires Mill Comparison" %}}
{{% header_link "Blowing Desert" %}}
{{% next_link "Atlantis Stargate" %}}
{{< video path="img/BlowingDesert" alt="Abandoned site in a blowing desert" >}}
{{< /render_img >}}

{{< render_img
  description="A Stargate was one of the first things I wanted to make. Finally got around to it!"
  renderdate="2021-4-20"
  rendertime="50 seconds"
  samples="200"
  engine="Cycles (with denoiser node)" >}}
{{% previous_link "Blowing Desert" %}}
{{% header_link "Atlantis Stargate" %}}
{{% next_link "Crazy Concentric Voronoi (from tute)" %}}
{{< image path="img/AtlantisStargate" alt="Stargate from SG:A with some Wraith Darts incoming" >}}
{{< /render_img >}}

{{< render_img
  description="Quick run to test this crazy voronoi displacement I saw on Reddit"
  renderdate="2021-4-15"
  rendertime="60 minutes"
  samples="10"
  engine="Cycles (with denoiser node)"
  frames="250"
  tutorialUrl="https://www.youtube.com/watch?v=_bdcQXMQ4e0"
  tutorialText="CG Matter" >}}
{{% previous_link "Atlantis Stargate" %}}
{{% header_link "Crazy Concentric Voronoi (from tute)" %}}
{{% next_link "Futuristic Star Trek style table" %}}
{{< video path="img/ConcentricVoronoi" alt="Some crazy vector node stuff to get this cool voronoi columns effect" >}}
{{< /render_img >}}

{{< render_img 
  description="So much gloss and transparency. There's loads of work in stuff that can't even be seen (e.g. pistons on lifter thing)" 
  renderdate="2021-04-07" 
  rendertime="23 hours" 
  samples="100" 
  frames="1320" >}}
{{% previous_link "Crazy Concentric Voronoi (from tute)" %}}
{{% header_link "Futuristic Star Trek style table" %}}
{{% next_link "Marbles and Pebbles" %}}
{{< video path="img/ScifiTable" alt="Futuristic Touch Table, with some shameless self advertising" >}}
{{< /render_img >}}

{{< render_img 
  description="More geometry nodes, with marbles!" 
  renderdate="2021-03-09" 
  rendertime="60 seconds" 
  samples="300" >}}
{{% previous_link "Futuristic Star Trek style table" %}}
{{% header_link "Marbles and Pebbles" %}}
{{% next_link "Geometry Node Station" %}}
{{< image path="img/MarblesOnPebbledGround" alt="Does anyone have marbles anymore?" >}}
{{< /render_img >}}

{{< render_img 
  description="Quick space station using the new Geometry Nodes" 
  renderdate="2021-03-01" 
  rendertime="20 seconds" 
  samples="500" >}}
{{% previous_link "Marbles and Pebbles" %}}
{{% header_link "Geometry Node Station" %}}
{{% next_link "Floating Rock Platform" %}}
{{< image path="img/GeoNodeStation" alt="Kinda looks like a metal rubbish dump all stuck together" >}}
{{< /render_img >}}

{{< render_img 
  description="I like floating platforms. I also like volumetrics." 
  rendertime="20 minutes" 
  samples="300" 
  renderdate="2021-02-28" >}}
{{% previous_link "Geometry Node Station" %}}
{{% header_link "Floating Rock Platform" %}}
{{% next_link "Control Service Weapon" %}}
{{< image path="img/FloatingRockPlatform" alt="Floating rocks in the glowing mist" >}}
{{< /render_img >}}

{{< render_img 
  description="Control is a fun game. I wanted to play with the Service Weapon in Blender"
  rendertime="60 minutes" 
  samples="200" 
  renderdate="2021-02-02" 
  frames="240" >}}
{{% previous_link "Floating Rock Platform" %}}
{{% header_link "Control Service Weapon" %}}
{{% next_link "My Subnautica Base" %}}
{{< video path="img/ControlServiceWeapon_Spinning" alt="Animation is hard, so this was good practice." >}}
{{< video path="img/ControlServiceWeapon_Still" alt="Another one with less motion." width="500px" >}}
{{< /render_img >}}


{{< render_img 
  description="Recently finished Subnautica and liked the aesthetic. Underwater bases are some James Bond villain levels of AWESOME :D" 
  rendertime="12 minutes" 
  samples="500" 
  renderdate="2020-12-04" >}}
{{% previous_link "Control Service Weapon" %}}
{{% header_link "My Subnautica Base" %}}
{{% next_link "Working on the stairs" %}}
{{< image path="img/SubnauticaBase" alt="Right on the edge of the Jellyshroom Caves!" >}}
{{< /render_img >}}

{{< render_img
  description="I wanted to try some procedural texture tiling. It's waaaay over my head though." 
  rendertime="13 minutes"
  samples="800" 
  renderdate="2020-11-12" >}}
{{% previous_link "My Subnautica Base" %}}
{{% header_link "Working on the stairs" %}}
{{% next_link "Halloweeeeen" %}}
{{< image path="img/MetalStairs" alt="Such lighting!" >}}
{{< /render_img >}}


{{< render_img
  description="I've always liked these kinds of spooky atmospheric scenes." 
  rendertime="12 minutes" 
  samples="200"
  renderdate="2020-10-20" >}}
{{% previous_link "Working on the stairs" %}}
{{% header_link "Halloweeeeen" %}}
{{% next_link "Crystal Caverns" %}}
{{< image path="img/Spooky" alt="Spooooky Pumpkin fun with knife project" >}}
{{< /render_img >}}


{{< render_img 
  description="Trying some glowing crystal action" 
  rendertime="18 minutes" 
  samples="100, 1000 for volume" 
  engine="Cycles" 
  renderdate="2020-10-14" >}}
{{% previous_link "Halloweeeeen" %}}
{{% header_link "Crystal Caverns" %}}
{{% next_link "Mass Relay" %}}
{{< image path="img/CrystalCaverns" alt="Glowing crystals took a while, volume took even longer" >}}
{{< /render_img >}}


{{< render_img 
  description="Found this in the old boxes. My first foray into Blender, took months. Then about 5 hours to make it way better. Relay model isn't mine" 
  credits="Relay Model: SporeAltair@https://sketchfab.com/models/dbc16b9795234c6fa6c0a6ca8eaa4f00"
  rendertime="7 hours" 
  samples="100"
  engine="Cycles"
  frames="1510" 
  renderdate="2020-09-17" >}}
{{% previous_link "Crystal Caverns" %}}
{{% header_link "Mass Relay" %}}
{{% next_link "SCP TV" %}}
{{% video path="img/Relay" width="1280" height="720" alt="Asteroid fields, although unrealistic, feel awesome. As does space magic transportation." %}}  
{{< /render_img >}}


{{< render_img
  description="I love Control (Remedy game), and I wanted to play with boids" 
  rendertime="13 hours"
  samples="300" 
  engine="Cycles" 
  frames="500" 
  renderdate="2020-09-10" >}}
{{% previous_link "Mass Relay" %}}
{{% header_link "SCP TV" %}}
{{% next_link "University Water" %}}
{{% video path="img/ControlTV" width="1280" height="720" alt="Boids make this pretty nice, but they're hard to control" %}}  
{{< /render_img >}}


{{< render_img
  description="Playing with water and cloth sims" 
  rendertime="12 hours" 
  samples="400" 
  engine="Cycles" 
  frames="250" 
  renderdate="2020-09-04" >}}
{{% previous_link "SCP TV" %}}
{{% header_link "University Water" %}}
{{% next_link "Borg Cube" %}}
{{% video path="img/WaterAndCloth" width="1280" height="720" alt="Water and cloth sims take ages" %}}  
{{< /render_img >}}


{{< render_img 
  description="I got a decent way through making this without a tutorial, so I'm pleased. The JSDisplacement greebles and AO emmision methods are really cool." 
  rendertime="4.5 minutes" 
  samples="400" 
  engine="Cycles" 
  renderdate="2020-08-07" 
  tutorialUrl="https://www.blendernation.com/2020/04/08/create-a-borg-cube-using-displacements/" 
  tutorialText="By Dikko" >}}
{{% previous_link "University Water" %}}
{{% header_link "Borg Cube" %}}
{{% next_link "Shattered World" %}}
{{< image path="img/BorgCube" alt="Borg Cube - you will be assimilated" >}}
{{< /render_img >}}


{{< render_img 
  description="Wanted to make a shattered world for a while. I'm not happy with this, but it's a start for next time. That volume tanked the rendertime too..." 
  rendertime="20.5 minutes" 
  samples="400" 
  engine="Cycles" 
  renderdate="2020-08-07" >}}
{{% previous_link "Borg Cube" %}}
{{% header_link "Shattered World" %}}
{{% next_link "Bullets" %}}
{{< image path="img/ShatteredPlanet" alt="Shattered World with dust cloud" >}}
{{< /render_img >}}


{{< render_img 
  description="Playing Halo MCC with a friend and figured it'd be easy to do that menu scene with all the bullets ¯\_(ツ)_/¯"
  rendertime="6 minutes" 
  samples="800" 
  engine="Cycles" 
  renderdate="2020-08-07" >}}
{{% previous_link "Shattered World" %}}
{{% header_link "Bullets" %}}
{{% next_link "Tea and Biscuits" %}}
{{< image path="img/Bullets" alt="Bullets scattered everywhere on a table" >}}
{{< /render_img >}}


{{< render_img
  description="The teapot was the hardest thing to mode. Feels like I did it the hardest way possible."
  rendertime="3.5 minutes" 
  samples="300" 
  engine="Cycles" 
  renderdate="2020-08-07" >}}
{{% previous_link "Bullets" %}}
{{% header_link "Tea and Biscuits" %}}
{{% next_link "Well and Bucket" %}}
{{< image path="img/PoyasTea" alt="Tea and biscuits scene from a colleagues' photo" >}}
{{< /render_img >}}


{{< render_img 
  description="Proving to myself I COULD do rocks with displacements, also the rope and grass." 
  rendertime="11 minutes" 
  samples="400" 
  engine="Cycles"
  renderdate="2020-08-07" >}}
{{% previous_link "Tea and Biscuits" %}}
{{% header_link "Well and Bucket" %}}
{{% next_link "The Beast Planet" %}}
{{< image path="img/CrackedWell" alt="Rocky well with a bucket and rope next to it" >}}
{{< /render_img >}}


{{< render_img
  description="I got to practice some rigging, animations, and compositing :D It's from a show called Shadow Raiders. Some good sci-fi ideas in there." 
  rendertime="2 hours"
  samples="400"
  engine="Cycles" 
  frames="600" 
  renderdate="2020-05-13" >}}
{{% previous_link "Well and Bucket" %}}
{{% header_link "The Beast Planet" %}}
{{% next_link "Ancient Door" %}}
{{% video path="img/ShadowRaiders" width="1280" height="720" alt="Shadow Raiders Beast Planet animation" %}}  
{{< /render_img >}}


{{< render_img
  description="Ancient areas are creepy places! I wanted to try my hand at animating. I need to do a tutorial..." 
  rendertime="5 hours"
  samples="64"
  engine="Eevee" 
  frames="1100" 
  renderdate="2020-05-08" >}}
{{% previous_link "The Beast Planet" %}}
{{% header_link "Ancient Door" %}}
{{% next_link "Happy Easter!" %}}
{{% video path="img/AncientDoor" width="1280" height="720" alt="Old dusty door animation" %}}  
{{< /render_img >}}


{{< render_img description="Eggster!" rendertime="1.5 minutes" samples="128" renderdate="2020-04-17" >}}
{{% previous_link "Ancient Door" %}}
{{% header_link "Happy Easter!" %}}
{{% next_link "Sword showing runes" %}}
{{< image path="img/Easter" alt="Easter Eggs" >}}
{{< /render_img >}}


{{< render_img 
  description="I did another one from the other side so I could see all those cool runes :D" 
  rendertime="5 seconds" 
  samples="64" 
  engine="Eevee" 
  renderdate="2020-04-06" >}}
{{% previous_link "Happy Easter!" %}}
{{% header_link "Sword showing runes" %}}
{{% next_link "Sword WITHOUT cinematic compositing" %}}
{{< image path="img/SwordRunesView" alt="Sword showing runes" >}}
{{< /render_img >}}   


{{< render_img
  description="I almost like this one better. Still, compositing can add much more flair than I gave it credit for."
  rendertime="5 seconds"
  samples="64" 
  engine="Eevee" 
  renderdate="2020-04-06" >}}
{{% previous_link "Sword showing runes" %}}
{{% header_link "Sword WITHOUT cinematic compositing" %}}
{{% next_link "Sword from CG Fasttrack" %}}
{{< image path="img/Sword_Original" alt="Sword WITHOUT compositing" >}}
{{< /render_img >}}


{{< render_img
  description="The CG Fasttrack tutorial sword. I ended up using different assets or making some out of stubborness. The final result suffers for it, but I learned many new things." 
  rendertime="5 seconds" 
  samples="64" 
  engine="Eevee" 
  renderdate="2020-04-06" 
  tutorialUrl="https://www.youtube.com/watch?v=zHv4VDoCwYc"
  tutorialText="CG Fasttrack" >}}
{{% previous_link "Sword WITHOUT cinematic compositing" %}}
{{% header_link "Sword from CG Fasttrack" %}}
{{% next_link "Enchanting table" %}}
{{< image path="img/Sword" alt="Sword with cinematic compositing" >}}
{{< /render_img >}}


{{< render_img description="My go at an enchanting table. Not really happy with it, but sometimes you're just done, you know?" rendertime="15 minutes" samples="500" renderdate="2020-02-29">}}
{{% previous_link "Sword from CG Fasttrack" %}}
{{% header_link "Enchanting table" %}}
{{% next_link "Urban Takeaway Coffee" %}}
{{< image path="img/EnchantingTable" alt="Enchanting table with some fun knick knacks" >}}
{{< /render_img >}}


{{< render_img 
  description="Coffee is the best thing, and sitting with one during the city buzz is very relaxing.| I was really happy with the bridge, but I realised that the DOF made it hard to see, so I did another one with less DOF :)"
  rendertime="15 minutes" 
  samples="500"
  renderdate="2020-02-02" >}}
{{% previous_link "Enchanting table" %}}
{{% header_link "Urban Takeaway Coffee" %}}
{{% next_link "Fixed olives, highlighted differences" %}}
{{< image path="img/TakeawayCoffee" alt="Takeaway coffee up on a wooden table, so relaxing" >}}
{{< image path="img/TakeawayCoffee_Less_DOF" alt="Less DOF, since I'm pleased with the bridge :)" width="500px" >}}
{{< /render_img >}}  


{{< render_img
  description="Finally fixed the olives. Note the differences mainly in the olives themselves, jar, and fork. I wasn't happy with the materials originally, and I want to show the difference now afterwards."
  rendertime="25 seconds"
  samples="100" 
  renderdate="2020-01-31" >}}
{{% previous_link "Urban Takeaway Coffee" %}}
{{% header_link "Fixed olives, highlighted differences" %}}
{{% next_link "Olives on a plate!" %}}
{{< image path="img/Olives_Difference" alt="Old olive render vs new bits" >}}
{{< /render_img >}}  


{{< render_img description="Olives are my favourite single food. Good inspiration." rendertime="25 seconds" samples="100" renderdate="2020-01-27" >}}
{{% previous_link "Fixed olives, highlighted differences" %}}
{{% header_link "Olives on a plate!" %}}
{{% next_link "Spooky" %}}
{{< image path="img/Olives" alt="Olives on a plate!" >}}
{{< /render_img >}}  


{{< render_img description="This was going to be a bright grassy scene, but then I needed to hide the bad grass." rendertime="~1 hour" samples="100" renderdate="2020-01-26" >}}
{{% previous_link "Olives on a plate!" %}}
{{% header_link "Spooky" %}}
{{% next_link "Ocean View" %}}
{{< image path="img/Grass" alt="Spooky :) Ground turned out really good" nospacer="true" >}}
{{< /render_img >}}  


{{< render_img description="I wanted to try making water with mist in the distance. I like it." rendertime="30 seconds" samples="30"  renderdate="2020-01-18">}}
{{% previous_link "Spooky" %}}
{{% header_link "Ocean View" %}}
{{% next_link "Floating Rocks" %}}
{{< image path="img/Ocean" alt="Bit of an ocean view" nospacer="true" >}}
{{< /render_img >}}  


{{< render_img description="I like floating rocks ¯\_(ツ)_/¯" rendertime="60 seconds" samples="20" renderdate="2020-01-18" >}}
{{% previous_link "Ocean View" %}}
{{% header_link "Floating Rocks" %}}
{{% next_link "Moovin Doonut" %}}
{{< image path="img/FloatingRocks" alt="Random floating rock" nospacer="true" >}}
{{< /render_img >}}  


{{< render_img description="Donut gettin spun into scene like a champ" rendertime="~2 minutes" samples="20" frames="60" renderdate="2020-01-18" >}}
{{% previous_link "Floating Rocks" %}}
{{% header_link "Moovin Doonut" %}}
{{% next_link "Doonut" %}}
{{% video path="img/Donut_WithMyMarble" width="1920" height="1080" alt="I can almost hear it" %}}  
{{< /render_img >}}


{{< render_img 
  description="Added a true Australian treat. Lamington :)" 
  rendertime="~30 seconds" 
  samples="20" 
  renderdate="2020-01-18" 
  tutorialUrl="https://www.youtube.com/watch?v=TPrnSACiTJ4"
  tutorialText="The Infamous BlenderGuru!" >}}
{{% previous_link "Moovin Doonut" %}}
{{% header_link "Doonut" %}}
{{< image path="img/Donut_WithMyMarble" alt="Obligatory Donut Render" nospacer="true" >}}  
{{< /render_img >}}  

</div>
</div>
