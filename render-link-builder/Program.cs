using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace render_link_builder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Pass the path to render Index.md");
                return;
            }

            // See if some of this can be merged

            var pathToRenderIndex = args[0];
            var contents = await File.ReadAllLinesAsync(pathToRenderIndex);

            var contentsWithoutNextOrPrevious = contents.Where(line => !line.Contains("next_link") && !line.Contains("previous_link"));
            var headerLinkLines = contentsWithoutNextOrPrevious.Where(line => line.Contains("header_link")).ToArray();

            var replacementEntries = headerLinkLines.Select((line, idx) =>
            {
                var previousLinkEntry = idx == 0 ? null : GetLinkFromLine(headerLinkLines[idx - 1]);
                var nextLinkEntry = idx == headerLinkLines.Length - 1 ? null : GetLinkFromLine(headerLinkLines[idx + 1]);

                var previousLink = previousLinkEntry == null ? null : $"{{{{% previous_link \"{previousLinkEntry}\" %}}}}";
                var nextLink = nextLinkEntry == null ? null : $"{{{{% next_link \"{nextLinkEntry}\" %}}}}";

                return new { previousLink, line, nextLink };
            }).ToDictionary(x => x.line);

            var newContents = contentsWithoutNextOrPrevious.SelectMany(line =>
            {
                if (!replacementEntries.ContainsKey(line))
                {
                    return new[] { line };
                }

                var replacement = replacementEntries[line];
                return new[] { replacement.previousLink, replacement.line, replacement.nextLink }
                    .Where(x => x != null);
            });

            await File.WriteAllLinesAsync(pathToRenderIndex, newContents);

            Console.WriteLine("Updated all render links!");
        }

        static string GetLinkFromLine(string line)
        {
            return line.Split('"')[1];
        }
    }
}
