using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace AssetOptimiser
{
    static partial class Program
    {
        static string ffmpegPath = null;
        static string cwepPath = null;
        static Format[] Formats = null;

        static async Task Main(string[] args)
        {
            if (!ProcessCmdLineArgs(args, out int crf, out int webpQuality, out string rootPath))
                return;

            Formats = new[]
            {
                new Format("AV1", "mp4", "libaom-av1", crf, 0, "-tiles 2x2 -row-mt 1 -strict experimental -movflags +faststart -cpu-used 0"),
                new Format("VP9", "webm", "libvpx-vp9", crf, 0, ""),
                new Format("x264", "mp4", "libx264", crf, null, "-movflags +faststart"),
            };

            ffmpegPath = GetFFMpegPath();
            cwepPath = GetCWebpPath();

            Console.WriteLine("Looking for unoptimised assets...");
            Console.WriteLine();

            var pictures = GetPictures(rootPath);
            var videos = GetVideos(rootPath);

            if (!pictures.Any() && !videos.Any())
            {
                Console.WriteLine("Nothing to convert!");
                return;
            }

            var picsText = pictures.Select(p => $"    {Path.Combine(p.Directory.Replace(rootPath, ""), p.FileName)}");
            var videosText = videos.Select(v => $"    {Path.Combine(v.Directory.Replace(rootPath, ""), v.FileName)}: {v.Format.Name}");

            Console.WriteLine("Preparing to convert the following files:");
            if (pictures.Any())
            {
                Console.WriteLine("Pictures:");
                foreach (var line in picsText)
                    Console.WriteLine(line);

                Console.WriteLine();
            }

            if (videos.Any())
            {
                Console.WriteLine("Videos:");
                foreach (var line in videosText)
                    Console.WriteLine(line);

                Console.WriteLine();
            }

            while(true)
            {
                Console.WriteLine("Continue? [y/n]");
                var entry = Console.ReadLine();
                if (entry.Contains("y", StringComparison.OrdinalIgnoreCase))
                    break;

                if (entry.Contains("n", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            if (pictures.Any())
                await ConvertPictures(pictures, webpQuality);
            else
                Console.WriteLine("No images to convert!");

            if (videos.Any())
                await ConvertVideos(videos);
            else
                Console.WriteLine("No videos to convert!");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("~~~~~ Completed! ~~~~~");
            Console.ReadLine();
        }

        static bool ProcessCmdLineArgs(string[] args, out int crf, out int webpQuality, out string rootPath)
        {
            crf = 30;
            webpQuality = 90;
            if (args?.Any() != true)
            {
                Console.WriteLine("1: Root path to search, 2: (optional) CRF 0-51, 3: (optional) Q 1-100");
                rootPath = null;
                return false;
            }

            rootPath = args[0];
            if (args.Length > 1)
            {
                foreach(var arg in args.Skip(1))
                {
                    (var crfq, var webp) = ProcessArg(arg);
                    if (crfq.HasValue) crf = crfq.Value;
                    if (webp.HasValue) webpQuality = webp.Value;
                }
            }

            return true;
        }

        static (int? CRF, int? webpQuality) ProcessArg(string arg)
        {
            var isCRF = arg.ToLower().Contains("crf");
            if (isCRF)
            {
                if (!int.TryParse(arg.Replace("--crf=", ""), out int crf))
                    throw new ArgumentException("CRF is not formatted right. Should be '--crf=0-51'");

                return (crf, null);
            }

            var isQ = arg.ToLower().Contains("webpq");
            if (isQ)
            {
                if (!int.TryParse(arg.Replace("--webpq=", ""), out int webpQ))
                    throw new ArgumentException("webpQ is not formatted right. Should be '--webpq=1-100'");

                return (null, webpQ);
            }

            return (null, null);
        }

        static IEnumerable<(string Directory, string FileName)> GetPictures(string rootPath)
        {
            var pngs = Directory.GetFiles(rootPath, "*.png", SearchOption.AllDirectories)
                .Select(x => new { Directory = Path.GetDirectoryName(x), FileName = Path.GetFileName(x), NoExtension = Path.GetFileNameWithoutExtension(x)});

            var webps = Directory.GetFiles(rootPath, "*.webp", SearchOption.AllDirectories)
                .Select(x => new { Directory = Path.GetDirectoryName(x), FileName = Path.GetFileName(x), NoExtension = Path.GetFileNameWithoutExtension(x) });


            return pngs.Where(p => !webps.Any(w => w.NoExtension == p.NoExtension))
                .Select(f => (f.Directory, f.FileName));
        }

        static IEnumerable<VideoJob> GetVideos(string rootPath)
        {
            bool isUnOptimised(string filename) => !Formats
                .Any(format => !format.HasPostFix ? false : Path.GetFileNameWithoutExtension(filename).EndsWith(format.PostFix));

            string formatDestinationFilename(string fullpath, Format format) => $"{Path.GetFileNameWithoutExtension(fullpath)}{format.PostFix}.{format.Extension}";


            var allVideos = Directory.GetFiles(rootPath, "*.avi", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rootPath, "*.mp4", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(rootPath, "*.webm", SearchOption.AllDirectories));

            var jobs = allVideos
                .Where(isUnOptimised)
                .SelectMany(video => Formats.Select(f => new VideoJob
                {
                    Directory = Path.GetDirectoryName(video),
                    FileName = Path.GetFileName(video),
                    Format = f,
                    DestinationFileName = formatDestinationFilename(video, f),
                }));


            return jobs
                .Where(j => !allVideos.Contains(Path.Join(j.Directory, j.DestinationFileName)))
                .Where(x => !File.Exists(Path.Combine(x.Directory, x.DestinationFileName)));
        }


        static async Task ConvertPictures(IEnumerable<(string Directory, string FileName)> pictures, int webpQuality)
        {
            Console.WriteLine($"Converting {pictures.Count()} pictures...");
            foreach (var picture in pictures)
            {
                var newPath = $"{Path.GetFileNameWithoutExtension(picture.FileName)}.webp";
                var arg = $"{picture.FileName} -o {newPath} -mt -m 6 -pass 10 -q {webpQuality}";
                await StartProcess(cwepPath, picture.Directory, arg);
            }
        }

        static async Task ConvertVideos(IEnumerable<VideoJob> jobs)
        {
            Console.WriteLine($"Converting {jobs.Count()} videos...");
            foreach (var job in jobs)
            {
                var bitrate = job.Format.Bitrate.HasValue ? $"-b:v {job.Format.Bitrate}" : "";
                var arg = $"-i {job.FileName} -c:v {job.Format.Encoder} -crf {job.Format.CRF} {bitrate} {job.Format.AdditionalArguments} {job.DestinationFileName}";
                await StartProcess(ffmpegPath, job.Directory, arg);
            }
        }

        static string GetFFMpegPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var chocolateyBinPath = path.Split(';').Single(x => x.Contains("chocol", StringComparison.OrdinalIgnoreCase));
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


        static string GetCWebpPath()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;

            var cwebp = Path.Combine(appRoot, "Webp", "bin", "cwebp.exe");
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
        static Task StartProcess(string tool, string workingDirectory, string argument)
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
