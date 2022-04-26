using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssetOptimiser
{
  public static class Helper
  {
    public static Format[] Formats = null;
    public static string ffmpegPath = null;
    public static string cwepPath = null;

    public static IEnumerable<PictureJob> GetPictures(string rootPath)
    {
      var pngs = Directory.GetFiles(rootPath, "*.png", SearchOption.AllDirectories)
        .Select(x => new { Directory = Path.GetDirectoryName(x), FileName = Path.GetFileName(x), NoExtension = Path.GetFileNameWithoutExtension(x) });

      var webps = Directory.GetFiles(rootPath, "*.webp", SearchOption.AllDirectories)
        .Select(x => new { Directory = Path.GetDirectoryName(x), FileName = Path.GetFileName(x), NoExtension = Path.GetFileNameWithoutExtension(x) });

      return pngs
        .Where(p => !webps.Any(w => w.NoExtension == p.NoExtension))
        .Select(f => new PictureJob(f.Directory, f.FileName));
    }

    public static IEnumerable<PictureJob> GetRenders(string renderPath)
    {
      var webps = Directory.GetFiles(renderPath, "*.webp", SearchOption.AllDirectories)
        .Select(x => new
        {
          Directory = Path.GetDirectoryName(x),
          FileName = Path.GetFileName(x),
          RenderName = Path.GetFileNameWithoutExtension(x).Replace("_halfsize", "")  // We can do this because atm, there's always normal and halfsize
        });

      var halfsize = Directory.GetFiles(renderPath, "*_halfsize.webp", SearchOption.AllDirectories)
        .Select(x => new
        {
          Directory = Path.GetDirectoryName(x),
          FileName = Path.GetFileName(x),
          RenderName = Path.GetFileNameWithoutExtension(x).Replace("_halfsize", "")
        });

      return webps
        .Where(x => !halfsize.Any(h => h.RenderName == x.RenderName))
        .Select(x => new PictureJob(x.Directory, x.FileName));
    }

    public static IEnumerable<VideoJob> GetVideos(string rootPath)
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

    public static async Task ConvertPictures(IEnumerable<PictureJob> pictures, int webpQuality)
    {
      Console.WriteLine($"Converting {pictures.Count()} pictures...");
      foreach (var picture in pictures.DistinctBy(x => x.FilenameNoExt))
      {
        var normalExec = picture.GetExecutionString(webpQuality, false);
        var halfsizeExec = picture.GetExecutionString(webpQuality, true);

        await StartProcess(cwepPath, picture.Directory, normalExec);
        await StartProcess(cwepPath, picture.Directory, halfsizeExec);
      }
    }

    public static async Task ConvertVideos(IEnumerable<VideoJob> jobs)
    {
      Console.WriteLine($"Converting {jobs.Count()} videos...");
      foreach (var job in jobs)
      {
        var bitrate = job.Format.Bitrate.HasValue ? $"-b:v {job.Format.Bitrate}" : "";
        var arg = $"-i {job.FileName} -c:v {job.Format.Encoder} -crf {job.Format.CRF} {bitrate} {job.Format.AdditionalArguments} {job.DestinationFileName}";
        await StartProcess(ffmpegPath, job.Directory, arg);
      }
    }

    public static string GetFFMpegPath()
    {
      var path = Environment.GetEnvironmentVariable("PATH");
      var chocoBits = path.Split(';').Where(x => x.Contains("chocolatey\\bin", StringComparison.OrdinalIgnoreCase)).ToArray();
      foreach (var bit in chocoBits)
      {
        Console.WriteLine(bit);
      }

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
      var exePath = Path.GetDirectoryName(System.Reflection
                        .Assembly.GetExecutingAssembly().Location);
      Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
      var appRoot = appPathMatcher.Match(exePath).Value;

      var cwebp = Path.Combine(appRoot, "Webp", "cwebp.exe");
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