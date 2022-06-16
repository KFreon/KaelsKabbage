using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetOptimiser
{
    record ConversionItem(string Directory, string FileName, string WithoutExtension, string RenderName = null);

    public static class ConvertHelper
    {
        public static Format[] VideoFormats { get; private set; }
        public static string ffmpegPath { get; private set; }
        public static string cwepPath { get; private set; }

        public static void Init(int? crf)
        {
            VideoFormats = new[]
            {
                new Format("AV1", "mp4", "libsvtav1", crf ?? 50, 0, "-movflags +faststart"),
                new Format("VP9", "webm", "libvpx-vp9", crf ?? 40, 0, "-row-mt 1 -tiles 2x2 -threads 8"),
            };

            ffmpegPath = GetFFMpegPath();
            cwepPath = GetCWebpPath();
        }

        public static IEnumerable<PictureJob> GetPictures(string rootPath, bool isRender) => 
            Directory.EnumerateFiles(rootPath, "*.png", SearchOption.AllDirectories)
                .Select(x => new ConversionItem(Path.GetDirectoryName(x), Path.GetFileName(x), Path.GetFileNameWithoutExtension(x)))
                .Select(f => new PictureJob(f.FileName, f.Directory, isRender));

        public static IEnumerable<VideoJob> GetVideos(string rootPath, bool isRender) =>
            Directory.GetFiles(rootPath, "*.mp4", SearchOption.AllDirectories)
                .Where(x => !x.Contains("_av1", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("_halfsize"))
                .SelectMany(video => VideoFormats.Select(f => new VideoJob(video, f, isRender)));

        public static async Task ConvertPictures(IEnumerable<PictureJob> pictures, int webpQuality)
        {
            Console.WriteLine($"Converting pictures...");
            foreach (var picture in pictures.DistinctBy(x => x.RootFilename))
            {
                var normalExec = picture.GetWebpExecutionString(webpQuality, false);
                var halfsizeExec = picture.GetWebpExecutionString(webpQuality, true);
                var postcardExec = picture.GetJpegExecutionString(webpQuality);

                if (!File.Exists(picture.FullWebpPath))
                {
                    await StartProcess(cwepPath, picture.Directory, normalExec);
                }

                if (picture.IsRender)
                {
                    if (!File.Exists(picture.HalfWebpPath))
                    {
                        await StartProcess(cwepPath, picture.Directory, halfsizeExec);
                    }

                    if (!File.Exists(picture.PostcardPath))
                    {
                        await StartProcess(ffmpegPath, picture.Directory, postcardExec);
                    }
                }
            }
        }

        public static async Task ConvertVideos(IEnumerable<VideoJob> jobs)
        {
            Console.WriteLine($"Converting videos...");
            foreach (var job in jobs)
            {
                var compressedExec = job.GetCompressedVideoExecutionString();
                var postcardExec = job.GetPostcardExecutionString();

                if (!File.Exists(job.FormattedPath))
                {
                    // Not doing AV1 conversions from mp4, do it from source instead
                    // Keeping this so I can regenerate when necessary if I don't have the source anymore
                    await StartProcess(ffmpegPath, job.Directory, compressedExec);
                }

                if (job.IsRender && !File.Exists(job.PostcardPath))
                {
                    await StartProcess(ffmpegPath, job.Directory, postcardExec);
                }
            }
        }

        public static string GetFFMpegPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var chocoBits = path.Split(';').Where(x => x.Contains("chocolatey\\bin", StringComparison.OrdinalIgnoreCase)).ToArray();

            var chocolateyBinPath = chocoBits.Single();
            var ffmpegPath = "";
            if (Directory.Exists(chocolateyBinPath))
            {
                ffmpegPath = Path.Combine(chocolateyBinPath, "ffmpeg.exe");
                if (!File.Exists(ffmpegPath))
                    throw new FileNotFoundException("ffmpeg is not installed at: " + ffmpegPath);
            }
            else
                throw new DirectoryNotFoundException("Choco directory not found on PATH");

            return ffmpegPath;
        }

        public static string GetCWebpPath()
        {
            var cwebp = Path.Combine(Core.Paths.ToolsPath, "Webp", "cwebp.exe");
            if (!File.Exists(cwebp))
                throw new FileNotFoundException("Webp converter not found at: " + cwebp);

            return cwebp;
        }


        /// <summary>
        /// NOT MULTITHREADED
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static Task StartProcess(string tool, string workingDirectory, string argument)
        {
            Console.WriteLine();
            Console.WriteLine($"Running: {Environment.NewLine}" +
                $"Tool: {Path.GetFileNameWithoutExtension(tool)} {Environment.NewLine}" +
                $"WorkingDir: {workingDirectory} {Environment.NewLine}" +
                $"Args: {argument}");

            var psi = new ProcessStartInfo(tool);
            psi.WorkingDirectory = workingDirectory;
            psi.CreateNoWindow = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            psi.Arguments = argument;
            var process = new Process()
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (sender, data) => Console.Write(data.Data);
            process.ErrorDataReceived += FFMpegOutputWriter;
            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        private static void FFMpegOutputWriter(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null)
                return;

            if (e.Data.StartsWith("frame="))
                Console.SetCursorPosition(0, Console.CursorTop - 1);

            Console.WriteLine(e.Data);
            Console.Out.Flush();
        }
    }
}