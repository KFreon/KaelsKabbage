using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetOptimiser
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            if (!CommandLineHelper.ProcessCmdLineArgs(args, out int? crf, out int webpQuality, out string rootPath))
                return;

            Console.WriteLine("Looking for unoptimised assets...");
            Console.WriteLine();

            // IN VS ONLY
#if DEBUG
            Core.Paths.BasePath = Path.GetDirectoryName(rootPath);
#endif

            var normalImages = ConvertHelper.GetPictures(rootPath, false);
            var renderImages = ConvertHelper.GetPictures(rootPath + "/Renders", true);
            var pictures = normalImages.Concat(renderImages);

            var normalVideos = ConvertHelper.GetVideos(rootPath, false);
            var renderVideos = ConvertHelper.GetVideos(rootPath + "/Renders", true);
            var videos = normalVideos.Concat(renderVideos);

            if (!pictures.Any() && !videos.Any())
            {
                Console.WriteLine("Nothing to convert!");
                return;
            }

            await ConvertFiles(crf, webpQuality, pictures, videos);
        }

        static async Task ConvertFiles(int? crf, int webpQuality, IEnumerable<PictureJob> pictures, IEnumerable<VideoJob> videos)
        {
            ConvertHelper.Init(crf);

            if (pictures.Any())
                await ConvertHelper.ConvertPictures(pictures, webpQuality);
            else
                Console.WriteLine("No images to convert!");

            if (videos.Any())
            {
                await ConvertHelper.ConvertVideos(videos);
            }
            else
                Console.WriteLine("No videos to convert!");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("~~~~~ Finished optimising assets! ~~~~~");
        }
    }
}
