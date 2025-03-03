using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;

// Below paths for VS
//var renders = "../../../../Content/Renders/index.md";

var destinationName = Path.Combine(Core.Paths.BasePath, "static/img/recent-render");
var destination = $"{destinationName}.png";

var renders = Directory.EnumerateFiles(Core.Paths.RendersFolder, "index.md", SearchOption.AllDirectories);

// If there are more than one on the same day, it can get the ordering wrong.
// So find all in the latest day, THEN get the files.
var latestRenders = renders.GroupBy(x => x[..10]).FirstOrDefault().Select(x => x).ToArray();

string latestRenderPath = null;
if (latestRenders.Length == 1) {
    latestRenderPath = latestRenders.FirstOrDefault();
}else{
    latestRenderPath = latestRenders.OrderByDescending(x => File.GetCreationTime(x)).FirstOrDefault();
}

var latestImage = Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(latestRenderPath), "img"), "*.png")
.FirstOrDefault();
var latestVideo = Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(latestRenderPath), "img"), "*_VP9.webm").FirstOrDefault();

if (File.Exists(latestImage)) {
    Console.WriteLine($"Processing {latestImage}");
    await Process.Start(Paths.FFMpegPath, $"-i \"{latestImage}\" -y -vf scale=275:-1 {destination}").WaitForExitAsync();
} else if (File.Exists(latestVideo)) {
    Console.WriteLine($"Processing {latestVideo}");
    await Process.Start(Paths.FFMpegPath, $"-i \"{latestVideo}\" -y -vframes 1 -vf scale=275:-1 {destination}").WaitForExitAsync();
}

// Convert to webp as well
await Process.Start(Paths.CwebpPath, $"-q 75 {destination} -o {destinationName}.webp").WaitForExitAsync();

Console.WriteLine("Latest Render = " + latestRenderPath);