using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

// Below paths for VS
//var allPosts = Directory.GetFiles("../../../../Content/Posts", "index.md", SearchOption.AllDirectories);
//var renders = "../../../../Content/Renders/index.md";

var allPosts = Directory.GetFiles("../Content/Posts", "index.md", SearchOption.AllDirectories);
var renders = "../Content/Renders/index.md";

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
      tags = post.tags ?? Array.Empty<string>(),
      href = "/post/" + Regex.Replace(post.slug.ToLowerInvariant(), "[^0-9a-z]", "-"),
      isRender = false
  });

var renderIndexEntries = File.ReadAllLines(renders)
  .Where(line => line.StartsWith("{{% header_link"))
  .Select(line => line.Split('"')[1])
  .Select(x => new
  {
      title = x,
      tags = Array.Empty<string>(),
      href = "/renders/#" + Regex.Replace(x.ToLowerInvariant(), "[^0-9a-z]", "-"),
      isRender = true
  });

var allEntries = postIndexEntries.Concat(renderIndexEntries).ToArray();
var serialised = JsonSerializer.Serialize(allEntries);
File.WriteAllText("../static/js/PagesIndex.json", serialised);