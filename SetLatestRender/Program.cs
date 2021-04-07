using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Below paths for VS
//var renders = "../../../../Content/Renders/index.md";

var renders = "../Content/Renders/index.md";
var destination = "../static/img/recent-render.png";

var latestRenderPath = File.ReadAllLines(renders)
    .First(x => x.StartsWith("{{< image") || x.StartsWith("{{< video"))
    .Split('"')[1];

var latestImage = "../Content/Renders/" + latestRenderPath + ".png";
var latestVideo = "../Content/Renders/" + latestRenderPath + "_VP9.webm";

if (File.Exists(latestImage)) {
    Process.Start("ffmpeg", $"-i {latestImage} -y -vf scale=300:-1 {destination}");
} else if (File.Exists(latestVideo)) {
    Process.Start("ffmpeg", $"-i {latestVideo} -y -vframes 1 -vf scale=300:-1 {destination}");
}

Console.WriteLine("Latest Render = " + latestRenderPath);