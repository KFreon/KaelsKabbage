using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AssetOptimiser
{
    public class VideoService 
    {
        public static Format[] VideoFormats { get; private set; }

        public VideoService(int? crf) 
        {
            VideoFormats = new[]
            {
                new Format("AV1", "mp4", "libsvtav1", crf ?? 45, 0, "-movflags +faststart"),
                new Format("VP9", "webm", "libvpx-vp9", crf ?? 35, 0, "-row-mt 1 -tiles 2x2 -threads 8"),
            };
        }

        public List<VideoJob> GetVideos(string rootPath, bool isRender)
        {
            return Directory.GetFiles(rootPath, "*.mp4", SearchOption.AllDirectories)
                .Where(x => !x.Contains("_av1", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("_halfsize"))
                .Where(x => !x.Contains("_quartersize"))
                .SelectMany(video => VideoFormats.Select(f => new VideoJob(video, f, isRender))).ToList();
        }

        public async Task<List<string>> ValidateVideos(List<VideoJob> jobs)
        {
            // Use FFProbe to double check that the main video format is yuv420p
            // This format seems to be the only one supported by all browsers
            // yuv444p isn't supported on Safari, it seems

            // Also need to make sure it's not h/x265 which I get sometimes out of screenToGif

            Console.WriteLine("Validating video formats...");

            var videosWithFormatIssues = new List<string>();

            foreach (var job in jobs)
            {
                var output = new StringBuilder();
                await FFMpegProcessor.StartProcess(Paths.FFProbePath, job.Directory, $"-i {job.FileName} -show_streams", output);

                // output has a load of info, but we're only looking for pix_fmt
                var lines = output.ToString().Split(Environment.NewLine).Select(x => x.Split('='));
                var pixelFormat = lines.FirstOrDefault(x => x[0] == "pix_fmt");
                var codec = lines.FirstOrDefault(x => x[0] == "codec_name");
                if (pixelFormat?.Any() == true && pixelFormat[1] != "yuv420p")
                    videosWithFormatIssues.Add($"{job.FileName} - {pixelFormat[1]}");
                else if (codec?.Any() == true && codec[1] == "hevc")
                    videosWithFormatIssues.Add($"{job.FileName} - hevc");
            }

            return videosWithFormatIssues;
        }

        public async Task ConvertVideos(List<VideoJob> jobs)
        {
            Console.WriteLine($"Converting videos...");
            foreach (var job in jobs)
            {
                var normal = job.GetCompressedVideoExecutionString(Size.Normal);
                var half = job.GetCompressedVideoExecutionString(Size.Halfsize);
                var quarter = job.GetCompressedVideoExecutionString(Size.Quartersize);
                var postcardExec = job.GetPostcardExecutionString();

                if (!File.Exists(job.FormattedPath(Size.Normal)))
                {
                    // Not doing AV1 conversions from mp4, do it from source instead
                    // Keeping this so I can regenerate when necessary if I don't have the source anymore
                    await FFMpegProcessor.StartProcess(Paths.FFMpegPath, job.Directory, normal);
                }

                if (!File.Exists(job.FormattedPath(Size.Halfsize)))
                {
                    // Not doing AV1 conversions from mp4, do it from source instead
                    // Keeping this so I can regenerate when necessary if I don't have the source anymore
                    await FFMpegProcessor.StartProcess(Paths.FFMpegPath, job.Directory, half);
                }

                if (!File.Exists(job.FormattedPath(Size.Quartersize)))
                {
                    // Not doing AV1 conversions from mp4, do it from source instead
                    // Keeping this so I can regenerate when necessary if I don't have the source anymore
                    await FFMpegProcessor.StartProcess(Paths.FFMpegPath, job.Directory, quarter);
                }

                if (job.IsRender && !File.Exists(job.PostcardPath))
                {
                    await FFMpegProcessor.StartProcess(Paths.FFMpegPath, job.Directory, postcardExec);
                }
            }
        }
    }
}