var rendersPath = "../../content/Renders";
var renderDump = $"../../.RENDER_DUMP";

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

foreach (var renderPath in consideredRenders) { 
  var render = new Render(renderPath, rendersPath, imageTemplate, videoTemplate);
  render.AssociatedRenders.AddRange(renders.Where(x => x != renderPath && x.Contains(render.NameWithoutExtension)).Select(x => new Render(x, rendersPath, imageTemplate, videoTemplate)));

  Directory.CreateDirectory(render.DestFolder)
    .CreateSubdirectory("img");

  MoveRender(render);

  var populatedItemTemplate = render.ItemTemplate.Replace("%path%", $"img/{render.NameWithoutExtension}");
  var fullTemplate = renderTemplate
    .Replace("%renderdate%", render.CreationDateAsString)
    .Replace("%headerlink%", render.FormattedName)
    .Replace("%title%", render.FormattedName)
    .Replace("%slug%", render.FormattedName.Replace(" ", "-").ToLower())
    .Replace("%date%", render.CreationDate.ToString("O"))
    .Replace("%frames%", render.IsVideo ? "frames=\"\"" : "")
    .Replace("%CONTENT%", populatedItemTemplate);

  File.WriteAllText(Path.Combine(render.DestFolder, "index.md"), fullTemplate);
  Console.WriteLine($"Created: {render.FormattedName}");
}



static void MoveRender(Render render)
{
  File.Move(render.FullPath, render.DestRender);

  foreach (var associated in render.AssociatedRenders)
  {
    File.Move(associated.FullPath, associated.DestRender);
  }
}