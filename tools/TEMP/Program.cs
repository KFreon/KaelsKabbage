using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

var rendersPath = "../../content/Renders";
var renders = Directory.EnumerateFiles(rendersPath, "*.*", SearchOption.AllDirectories);

var validExts = new[] {".webm", ".png"};

var validRenders = renders
  .Where(x => validExts.Contains(Path.GetExtension(x)));

foreach(var render in validRenders) {
  var folder = Path.GetDirectoryName(render);
  var destName = Path.GetFileNameWithoutExtension(render.Replace("_VP9", "")) + "_postcard.jpg";
  var destination = Path.Combine(folder, destName);

  if (Path.GetExtension(render) == ".png") {
    await Process.Start("ffmpeg", $"-i {render} -y -vf scale=275:-1 {destination}").WaitForExitAsync();
  } else {
      await Process.Start("ffmpeg", $"-i {render} -y -vframes 1 -vf scale=275:-1 {destination}").WaitForExitAsync();
  }
}