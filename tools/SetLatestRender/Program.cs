using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Below paths for VS
//var renders = "../../../../Content/Renders/index.md";

var renders = Path.Combine(Core.Paths.RendersFolder, "Renders/index.md");
var destinationName = Path.Combine(Core.Paths.BasePath, "static/img/recent-render");
var destination = $"{destinationName}.png";

var latestRenderPath = File.ReadAllLines(renders)
    .First(x => x.StartsWith("{{< image") || x.StartsWith("{{< video"))
    .Split('"')[1];

var latestImage = Path.Combine(Core.Paths.RendersFolder, latestRenderPath + ".png");
var latestVideo = Path.Combine(Core.Paths.RendersFolder, latestRenderPath + "_VP9.webm");

if (File.Exists(latestImage)) {
    await Process.Start("ffmpeg", $"-i {latestImage} -y -vf scale=275:-1 {destination}").WaitForExitAsync();
} else if (File.Exists(latestVideo)) {
    await Process.Start("ffmpeg", $"-i {latestVideo} -y -vframes 1 -vf scale=275:-1 {destination}").WaitForExitAsync();
}

// Convert to webp as well
await Process.Start(
        Path.Combine(Core.Paths.RendersFolder, "tools/NewAssetOptimiser/Webp/cwebp.exe"), 
        $"-q 75 {destination} -o {destinationName}.webp"
    ).WaitForExitAsync();

Console.WriteLine("Latest Render = " + latestRenderPath);