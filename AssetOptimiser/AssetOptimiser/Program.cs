using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetOptimiser
{
    class Program
    {
        static string ChocoPath = null;

        struct Format
        {
            public string Name { get; }
            public string Encoder { get; }
            public int CRF { get; }
            public int? Bitrate { get; }
            public string AdditionalArguments { get; }
            public string Extension { get; }
            public string PostFix => Name=="x264" ? "" : $"_{Name}";
            public bool HasPostFix => !string.IsNullOrEmpty(PostFix);

            public Format(string name, string extension, string encoder, int crf, int? bitrate, string additionalArguments)
            {
                Name = name;
                Extension = extension;
                Encoder = encoder;
                CRF = crf;
                Bitrate = bitrate;
                AdditionalArguments = additionalArguments;
            }
        }

        static Format[] Formats = null;

        


        static async Task Main(string[] args)
        {
            if (args?.Any() != true)
            {
                Console.WriteLine("1: Root path to search, 2: (optional) CRF 0-51");
                return;
            }

            var rootPath = args[0];
            var crf = 30;
            if (args.Length > 1)
            {
                if (!int.TryParse(args[1], out crf))
                    throw new ArgumentException("Second argument should be CRF, i.e. an int 0-51");
            }

            Formats = new[]
            {
                new Format("AV1", "mp4", "libaom-av1", crf, 0, "-tiles 2x2 -row-mt 1 -strict experimental -movflags +faststart"),
                new Format("VP9", "webm", "libvpx-vp9", crf, 0, ""),
                new Format("x264", "mp4", "libx264", crf, null, "-movflags +faststart"),
            };

            GetChocoBasePath();

            Console.WriteLine("Looking for unoptimised assets...");

            var pictures = GetPictures(rootPath);
            var videos = GetVideos(rootPath);

            if (pictures.Any())
                await ConvertPictures(pictures);
            else
                Console.WriteLine("No images to convert!");

            if (videos.Any())
                await ConvertVideos(videos);
            else
                Console.WriteLine("No videos to convert!");

            Console.WriteLine("Completed!");
        }

        static IEnumerable<(string Directory, string FileName)> GetPictures(string rootPath)
        {
            return Directory.GetFiles(rootPath, "*.png", SearchOption.AllDirectories)
                .Select(x => (Directory: Path.GetDirectoryName(x), FileName: Path.GetFileName(x)));
        }

        static IEnumerable<(string Directory, string FileName, Format[] RequiredFormats)> GetVideos(string rootPath)
        {
            bool isUnOptimised(string filename) => !Formats
                .Any(format => !format.HasPostFix ? false : Path.GetFileNameWithoutExtension(filename).EndsWith(format.PostFix));

            Format[] requiredFormats(string filename) => Path.GetExtension(filename) == ".mp4" ? Formats.Where(f => f.HasPostFix).ToArray() : Formats;


            var allVideos = Directory.GetFiles(rootPath, "*.avi", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rootPath, "*.mp4", SearchOption.AllDirectories));

            var allUnOptimised = allVideos.Where(isUnOptimised);
            // now remove those that already have encoded versions

            var encodedVideoNames = allVideos.Except(allUnOptimised)
                .Select(x =>
                {
                    var filename = Path.GetFileNameWithoutExtension(x);
                    foreach (var format in Formats.Where(f => f.HasPostFix))
                        filename = filename.Replace(format.PostFix, "");

                    return filename;
                });

            var videosToConvert = allUnOptimised
                .Where(vid => !encodedVideoNames.Contains(Path.GetFileNameWithoutExtension(vid)))
                .Select(x => (
                    Directory: Path.GetDirectoryName(x),
                    FileName: Path.GetFileName(x),
                    RequiredFormats: requiredFormats(x))
                )
                .ToList();

            return videosToConvert;
        }


        static async Task ConvertPictures(IEnumerable<(string Directory, string FileName)> pictures)
        {
            foreach (var picture in pictures)
            {
                var arg = ConstructWebpCmdLine(picture.FileName);
                await StartProcess("cwebp.exe", picture.Directory, arg);
            }
        }
        

        static async Task ConvertVideos(IEnumerable<(string Directory, string FileName, Format[] requiredFormats)> videos)
        {
            foreach (var video in videos)
            {
                foreach(var format in video.requiredFormats)
                {
                    var arg = ConstructFFMpegCmdLine(video.FileName, format);
                    await StartProcess("ffmpeg.exe", video.Directory, arg);
                }
            }
        }




        static string ConstructWebpCmdLine(string picturePath)
        {
            var newPath = $"{Path.GetFileNameWithoutExtension(picturePath)}.webm";
            return $"{picturePath}\" -o \"{newPath} -mt\"";
        }

        static string ConstructFFMpegCmdLine(string videoPath, Format format)
        {
            var newPath = $"{Path.GetFileNameWithoutExtension(videoPath)}{format.PostFix}.{format.Extension}";
            var bitrate = format.Bitrate.HasValue ? $"-b:v {format.Bitrate}" : "";
            return $"-i {videoPath} -c:v {format.Encoder} -crf {format.CRF} {bitrate} {format.AdditionalArguments} {newPath}";
        }




        static void GetChocoBasePath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var chocolateyBinPath = path.Split(';').Single(x => x.Contains("chocol", StringComparison.OrdinalIgnoreCase));
            if (Directory.Exists(chocolateyBinPath))
                ChocoPath = chocolateyBinPath;
            else
                throw new DirectoryNotFoundException("Choco directory not found on PATH");
        }

        static Task StartProcess(string toolName, string workingDirectory, string argument)
        {
            var psi = new ProcessStartInfo(toolName);
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
            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine("------- ERROR: " + data.Data);

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            return tcs.Task;
        }
    }
}
