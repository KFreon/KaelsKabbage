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
            ConvertHelper.Init(crf);

            var normalImages = ConvertHelper.GetPictures(rootPath, false);
            var renderImages = ConvertHelper.GetPictures(rootPath + "/Renders", true);
            var pictures = normalImages.Concat(renderImages).ToList();

            var normalVideos = ConvertHelper.GetVideos(rootPath, false);
            var renderVideos = ConvertHelper.GetVideos(rootPath + "/Renders", true).DistinctBy(x => x.FileName).ToList();
            var videos = normalVideos.Concat(renderVideos).ToList();

            if (!pictures.Any() && !videos.Any())
            {
                Console.WriteLine("Nothing to convert!");
                return;
            }
            await ConvertPictures(webpQuality, pictures);
            await ConvertVideos(videos);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("~~~~~ Finished optimising assets! ~~~~~");
        }

        private static async Task ConvertVideos(List<VideoJob> videos)
        {
            if (videos.Any())
            {
                var invalid = await ValidateVideoFormats(videos);
                if (invalid)
                    Console.WriteLine("****INVALID VIDEOS****");
                else
                    await ConvertHelper.ConvertVideos(videos);
            }
            else
            {
                Console.WriteLine("No videos to convert!");
            }
        }

        private static async Task<bool> ValidateVideoFormats(List<VideoJob> videos)
        {
            var videosWithFormatIssues = await ConvertHelper.ValidateVideos(videos);
            if (videosWithFormatIssues.Any())
            {
                Console.WriteLine("**INVALID FORMAT** We want yuv420p and NOT h/x265");
                foreach (var video in videosWithFormatIssues)
                {
                    Console.WriteLine(video);
                }
            }

            return videosWithFormatIssues.Any();
        }

        private static async Task ConvertPictures(int webpQuality, List<PictureJob> pictures)
        {
            if (pictures.Any())
                await ConvertHelper.ConvertPictures(pictures, webpQuality);
            else
                Console.WriteLine("No images to convert!");
        }
    }
}
