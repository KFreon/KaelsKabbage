using AssetOptimiser;
using System.Diagnostics;

var rendersPath = Core.Paths.RendersFolder;
var renderDump = Core.Paths.RenderDump;

var renderTemplate = File.ReadAllText("./render-img.md");
var videoTemplate = File.ReadAllText("./video.md");
var imageTemplate = File.ReadAllText("./image.md");
var renders = Directory.EnumerateFiles(renderDump);
var consideredRenders = renders.Where(x => x.EndsWith(".png") || x.EndsWith(".mp4"));  // Only use the raw files, we'll collect the rest later

Console.WriteLine($"----- Render Creator -----");

var numberOfRenders = consideredRenders.Count();
if (numberOfRenders > 0)
  Console.WriteLine($"Creating render posts: {numberOfRenders}");
else
  Console.WriteLine("No renders need creating.");

var parsedRenders = new List<Render>();
foreach (var renderPath in consideredRenders) { 
  var render = new Render(renderPath, rendersPath, imageTemplate, videoTemplate);
  render.AssociatedRenders.AddRange(renders.Where(x => x != renderPath && x.Contains(render.NameWithoutExtension)).Select(x => new Render(x, rendersPath, imageTemplate, videoTemplate)));

  parsedRenders.Add(render);
  Directory.CreateDirectory(render.DestFolder)
    .CreateSubdirectory("img");

  MoveRender(render);

  var populatedItemTemplate = render.ItemTemplate.Replace("%path%", $"img/{render.NameWithoutExtension}");
  var fullTemplate = renderTemplate
    .Replace("%renderdate%", render.CreationDateAsString)
    .Replace("%title%", render.FormattedName)
    .Replace("%slug%", render.FormattedName.Replace(" ", "-").ToLower())
    .Replace("%date%", render.CreationDate.ToString("O"))
    .Replace("%frames%", render.IsVideo ? "frames=\"\"" : "")
    .Replace("%CONTENT%", populatedItemTemplate);

  File.WriteAllText(Path.Combine(render.DestFolder, "index.md"), fullTemplate);
  await BuildPostCard(render.DestRender);
  Console.WriteLine($"Created: {render.FormattedName}");
}

var targets = parsedRenders.Select(x => new PictureJob(x.DestFolder, x.DestName));

// halfsizes, optimised images
await AssetOptimiser.Program.ConvertFiles(null, 90, Core.Paths.RendersFolder, targets, null);

// Open them all for us to adjust
foreach(var item in parsedRenders)
{
  Process.Start(Path.Combine(item.DestFolder, "index.md"));
}

Console.WriteLine($"-*-*FINISHED*-*-");







static void MoveRender(Render render)
{
  File.Move(render.FullPath, render.DestRender);

  foreach (var associated in render.AssociatedRenders)
  {
    File.Move(associated.FullPath, associated.DestRender);
  }
}

static async Task BuildPostCard(string render)
{
  var folder = Path.GetDirectoryName(render);
  var destName = Path.GetFileNameWithoutExtension(render.Replace("_VP9", "")) + "_postcard.jpg";
  var destination = Path.Combine(folder, destName);

  if (Path.GetExtension(render) == ".png")
  {
      await Process.Start("ffmpeg", $"-i {render} -y -vf scale=275:-1 {destination}").WaitForExitAsync();
  }
  else
  {
    await Process.Start("ffmpeg", $"-i {render} -y -vframes 1 -vf scale=275:-1 {destination}").WaitForExitAsync();
  }
}
