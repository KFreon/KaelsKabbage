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
      if (!ProcessCmdLineArgs(args, out int? crf, out int webpQuality, out string rootPath))
        return;

      Console.WriteLine("Looking for unoptimised assets...");
      Console.WriteLine();

      var normalImages = Helper.GetPictures(rootPath);
      var renders = Helper.GetRenders(rootPath + "/Renders");
      var pictures = normalImages.Concat(renders);

      var videos = Helper.GetVideos(rootPath);

      await ConvertFiles(crf, webpQuality, rootPath, pictures, videos);
    }

    static bool ProcessCmdLineArgs(string[] args, out int? crf, out int webpQuality, out string rootPath)
    {
      crf = null;
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
        foreach (var arg in args.Skip(1))
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

    public static async Task ConvertFiles(int? crf, int webpQuality, string rootPath, IEnumerable<PictureJob> pictures, IEnumerable<VideoJob> videos)
    {
      Helper.Formats = new[]
        {
          // Disabling for now since there's some issues to work out
          new Format("AV1", "mp4", "libaom-av1", crf ?? 50, 0, "-row-mt 1 -strict experimental -tile-columns 2 -threads 8 -pix_fmt yuv444p -movflags +faststart"), // yuv444 - can't have 12 bit colour - pngs seem to be 12 bit colour :(
          new Format("VP9", "webm", "libvpx-vp9", crf ?? 40, 0, "-row-mt 1 -tiles 2x2 -threads 8"),
          new Format("x264", "mp4", "libx264", crf ?? 30, null, "-row-mt 1 -tiles 2x2 -threads 8 -movflags +faststart"),
        };

      Helper.ffmpegPath = Helper.GetFFMpegPath();
      Helper.cwepPath = Helper.GetCWebpPath();

      if (!pictures.Any() && !videos.Any())
      {
        Console.WriteLine("Nothing to convert!");
        return;
      }

      var picsText = pictures.Select(p => $"    {Path.Combine(p.Directory.Replace(rootPath, ""), p.Filename)}");
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
        Console.WriteLine("Videos (not converting, do that from sources):");
        foreach (var line in videosText)
          Console.WriteLine(line);

        Console.WriteLine();
      }

      if (pictures.Any())
        await Helper.ConvertPictures(pictures, webpQuality);
      else
        Console.WriteLine("No images to convert!");

      if (videos.Any()) {
        await Helper.ConvertVideos(videos);
        await Helper.BuildVideoPostCards(videos);
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
