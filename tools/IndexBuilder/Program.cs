using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Core;

const string frontmatterDelimeter = "---";

// Below paths for VS
//var allPosts = Directory.GetFiles("../../../../Content/Posts", "index.md", SearchOption.AllDirectories);
//var renders = "../../../../Content/Renders/index.md";

var basePath = Environment.GetCommandLineArgs()[1];
var posts = Directory.GetFiles(Paths.PostsFolder, "index.md", SearchOption.AllDirectories);
var renders = Directory.GetFiles(Paths.RendersFolder, "index.md", SearchOption.AllDirectories);


Func<string[], string, string> parseFrontMatterEntry = (string[] frontMatter, string key) =>
{
    var entry = frontMatter.FirstOrDefault(x => x.StartsWith(key))?.Split(':', 2);
    if (entry is null || entry.Length < 2)
        return null;
    else
        return entry[1].Replace("\"", "").Trim();
};

var postIndexEntries = posts.Concat(renders)
  .Select(filePath => new 
  { 
    IsRender = filePath.Contains("renders", StringComparison.OrdinalIgnoreCase), 
    Lines = File.ReadAllLines(filePath) 
  })
  .Select(item => new
    {
      IsRender = item.IsRender,
      FrontMatter = item.Lines
        .Skip(1)
        .TakeWhile(x => x != frontmatterDelimeter)
        .ToArray(),
      Lines = item.Lines
    })
  .Select(item =>
  {
      var remainingContent = item.Lines.Skip(item.FrontMatter.Length + 1);
      var tags = parseFrontMatterEntry(item.FrontMatter, "tags")
        ?.Replace("[", "").Replace("]", "")
        .Split(',')
        .Select(x => x.Trim())
        .ToArray();

      bool.TryParse(parseFrontMatterEntry(item.FrontMatter, "draft"), out var draft);
      return new
      {
          title = parseFrontMatterEntry(item.FrontMatter, "title"),
          draft,
          slug = parseFrontMatterEntry(item.FrontMatter, "slug"),
          isRender = item.IsRender,
          tags,
          remainingContent
      };
  })
  .Where(post => !post.draft)
  .Select(post => new
  {
      post.title,
      post.tags,
      href = $"{(post.isRender ? "/renders/" : "/posts/")}{Regex.Replace(post.slug.ToLowerInvariant(), "[^0-9a-z]", "-")}".Replace("--", "-"),  // Occasionally, there were doubles that needed removal.
      post.isRender,
      post.remainingContent
  });

string[] commonWords = Array.Empty<string>();
string[] commonSymbols = new string[] {"\r", "\n", "\t", "-"};
var fullTextResults = postIndexEntries
  .Select(x => {
    string slashes = string.Empty;
    if (!x.isRender) {
      StringBuilder sb = new StringBuilder(string.Join(Environment.NewLine, x.remainingContent));
      foreach(var word in commonWords) {
        sb = sb.Replace(" " + word + " ", " ");
      }

      foreach(var symbol in commonSymbols) {
        sb = sb.Replace(symbol, "");
      }

      var spaces = Regex.Replace(sb.ToString(), @"\s+", " ");
      slashes = Regex.Replace(spaces, @"\+", "\\");
    }
    
    return new {
      x.href,
      x.isRender,
      x.title,
      x.tags,
      text = x.isRender ? string.Empty : slashes  // Don't want text for renders
    };
});

var serialiserOptions = new JsonSerializerOptions
{
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
};
var serialisedFullText = JsonSerializer.Serialize(fullTextResults, options: serialiserOptions);
var noUtf8 = Regex.Replace(serialisedFullText, @"[^\u0000-\u00FF]+", string.Empty);
File.WriteAllText(Path.Combine(basePath, "../static/search/FullText.json"), noUtf8);