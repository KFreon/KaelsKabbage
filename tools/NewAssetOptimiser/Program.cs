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

            Console.WriteLine($"CRF: {crf}");
            Console.WriteLine($"Webp: {webpQuality}");
            Console.WriteLine("Looking for unoptimised assets...");
            Console.WriteLine();

            // IN VS ONLY
#if DEBUG
            Core.Paths.BasePath = Path.GetDirectoryName(rootPath);
#endif

            var pictureService = new PictureService(webpQuality);
            var normalImages = pictureService.GetPictures(rootPath, false);
            var renderImages = pictureService.GetPictures(rootPath + "/renders", true);
            var pictures = normalImages.Concat(renderImages).ToList();

            var videoService = new VideoService(crf);
            var normalVideos = videoService.GetVideos(rootPath, false);
            var renderVideos = videoService.GetVideos(rootPath + "/renders", true).DistinctBy(x => x.FileName).ToList();
            var videos = normalVideos.Concat(renderVideos).ToList();

            if (!pictures.Any() && !videos.Any())
            {
                Console.WriteLine("Nothing to convert!");
                return;
            }
            await ConvertPictures(pictureService, pictures);
            await ConvertVideos(videoService, videos);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("~~~~~ Finished optimising assets! ~~~~~");
        }

        private static async Task ConvertVideos(VideoService videoService, List<VideoJob> videos)
        {
            if (videos.Any())
            {
                var videosWithFormatIssues = await videoService.ValidateVideos(videos);
                if (videosWithFormatIssues.Any())
                {
                    Console.WriteLine("**INVALID FORMAT** We want yuv420p and NOT h/x265");
                    foreach (var video in videosWithFormatIssues)
                        Console.WriteLine(video);
                }

                await videoService.ConvertVideos(videos);
            }
            else
            {
                Console.WriteLine("No videos to convert!");
            }
        }

        private static async Task ConvertPictures(PictureService service, List<PictureJob> pictures)
        {
            if (pictures.Any())
                await service.ConvertPictures(pictures);
            else
                Console.WriteLine("No images to convert!");
        }
    }
}
