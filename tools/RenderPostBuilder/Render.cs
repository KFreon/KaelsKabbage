using System.Text.RegularExpressions;

internal class Render {
  public string FullPath { get; set; }
  public string NameWithoutExtension { get; }
  public string FormattedName => Regex.Replace(NameWithoutExtension, "(.*)([A-Z])(.*)", "$1 $2$3");
  public string Extension { get; }
  public DateTime CreationDate { get; }
  public string CreationDateAsString => CreationDate.ToString("yyyy-MM-dd");
  public List<Render> AssociatedRenders { get; } = new List<Render>();
  public string ItemTemplate { get; set; }
  public string DestFolder { get; }
  public string DestRender { get; }
  public bool IsVideo { get; }

  public Render(string fullPath, string rendersPath, string imageTemplate, string videoTemplate)
  {
    FullPath = fullPath;
    NameWithoutExtension = Path.GetFileNameWithoutExtension(FullPath);
    Extension = Path.GetExtension(FullPath);
    CreationDate = File.GetLastWriteTime(FullPath);
    
    var directoryName = $"{CreationDateAsString}_{NameWithoutExtension}"
      .Replace("/", "-");
    DestFolder = Path.Combine(rendersPath, directoryName);
    DestRender = Path.Combine(DestFolder, "img", NameWithoutExtension).Replace("\\", "/") + $"{Extension}";


    if (fullPath.EndsWith(".png"))
    {
      ItemTemplate = imageTemplate;
    } 
    else if (fullPath.EndsWith(".mp4"))
    {
      ItemTemplate = videoTemplate;
      IsVideo = true;
    }
    else
    {
      throw new Exception($"Extension isn't allowed: {Extension}");
    }
  }
}