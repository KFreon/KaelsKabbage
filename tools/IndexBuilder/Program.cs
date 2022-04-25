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
var renders = Path.Combine(basePath, "Renders/index.md");

var allPosts = Directory.GetFiles(Path.Combine(basePath, "Posts"), "index.md", SearchOption.AllDirectories);

Func<IEnumerable<string>, string, string> parseFrontMatterEntry = (IEnumerable<string> frontMatter, string key) =>
{
    var entry = frontMatter.FirstOrDefault(x => x.StartsWith(key))?.Split(':', 2);
    if (entry is null || entry.Length < 2)
        return null;
    else
        return entry[1].Replace("\"", "").Trim();
};

var postIndexEntries = allPosts
  .Select(filePath => File.ReadAllLines(filePath))
  .Select(lines =>
    lines// FrontMatter
    .Skip(1)
    .Take(7))
  .Select(frontMatter =>
  {
      bool.TryParse(parseFrontMatterEntry(frontMatter, "draft"), out var draft);
      return new
      {
          title = parseFrontMatterEntry(frontMatter, "title"),
          draft = draft,
          slug = parseFrontMatterEntry(frontMatter, "slug"),
          tags = parseFrontMatterEntry(frontMatter, "tags")
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
      href = "/post/" + Regex.Replace(post.slug.ToLowerInvariant(), "[^0-9a-z]", "-"),
  });

var allEntries = postIndexEntries.Select(x => new 
{
  x.title,
  href = x.href.Replace("--", "-"),  // Occasionally, there were doubles that needed removal.
}).ToArray();
var serialised = "const pagesIndex = " + JsonSerializer.Serialize(allEntries) + ";";
File.WriteAllText(Path.Combine(basePath, "../static/js/PagesIndex.js"), serialised);