using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Below paths for VS
//var renders = "../../../../Content/Renders/index.md";

var destinationName = Path.Combine(Core.Paths.BasePath, "static/img/recent-render");
var destination = $"{destinationName}.png";

var renders = Directory.EnumerateFiles(Core.Paths.RendersFolder, "index.md", SearchOption.AllDirectories);

var latestRenderPath = renders.OrderByDescending(x => x).FirstOrDefault();

var latestImage = Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(latestRenderPath), "img"), "*.png").FirstOrDefault();
var latestVideo = Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(latestRenderPath), "img"), "*_VP9.webm").FirstOrDefault();

if (File.Exists(latestImage)) {
    await Process.Start("ffmpeg", $"-i {latestImage} -y -vf scale=275:-1 {destination}").WaitForExitAsync();
} else if (File.Exists(latestVideo)) {
    await Process.Start("ffmpeg", $"-i {latestVideo} -y -vframes 1 -vf scale=275:-1 {destination}").WaitForExitAsync();
}

// Convert to webp as well
await Process.Start(
        Path.Combine(Core.Paths.ToolsPath, "Webp/cwebp.exe"), 
        $"-q 75 {destination} -o {destinationName}.webp"
    ).WaitForExitAsync();

Console.WriteLine("Latest Render = " + latestRenderPath);