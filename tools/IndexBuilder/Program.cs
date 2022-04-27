using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

// Below paths for VS
//var allPosts = Directory.GetFiles("../../../../Content/Posts", "index.md", SearchOption.AllDirectories);
//var renders = "../../../../Content/Renders/index.md";

var basePath = Environment.GetCommandLineArgs()[1];
var posts = Directory.GetFiles(Path.Combine(basePath, "posts"), "index.md", SearchOption.AllDirectories);
var renders = Directory.GetFiles(Path.Combine(basePath, "renders"), "index.md", SearchOption.AllDirectories);

Func<IEnumerable<string>, string, string> parseFrontMatterEntry = (IEnumerable<string> frontMatter, string key) =>
{
    var entry = frontMatter.FirstOrDefault(x => x.StartsWith(key))?.Split(':', 2);
    if (entry is null || entry.Length < 2)
        return null;
    else
        return entry[1].Replace("\"", "").Trim();
};

var postIndexEntries = posts.Concat(renders)
  .Select(filePath => new { IsRender = filePath.Contains("/renders/"), Lines = File.ReadAllLines(filePath) })
  .Select(item =>
    new
    {
      IsRender = item.IsRender,
      FrontMatter = item.Lines
    .Skip(1)
    .Take(7)
    })
  .Select(item =>
  {
      bool.TryParse(parseFrontMatterEntry(item.FrontMatter, "draft"), out var draft);
      return new
      {
          title = parseFrontMatterEntry(item.FrontMatter, "title"),
          draft = draft,
          slug = parseFrontMatterEntry(item.FrontMatter, "slug"),
          isRender = item.IsRender,
          tags = parseFrontMatterEntry(item.FrontMatter, "tags")
        ?.Replace("[", "").Replace("]", "")
        .Split(',')
        .Select(x => x.Trim())
        .ToArray()
      };
  })
  .Where(post => !post.draft)
  .Select(post => new
  {
      post.title,
      href = (post.isRender ? "/posts/" : "/renders/") + Regex.Replace(post.slug.ToLowerInvariant(), "[^0-9a-z]", "-"),
  });

var allEntries = postIndexEntries.Select(x => new 
{
  x.title,
  href = x.href.Replace("--", "-"),  // Occasionally, there were doubles that needed removal.
}).ToArray();
var serialised = "const pagesIndex = " + JsonSerializer.Serialize(allEntries) + ";";
File.WriteAllText(Path.Combine(basePath, "../static/js/PagesIndex.js"), serialised);