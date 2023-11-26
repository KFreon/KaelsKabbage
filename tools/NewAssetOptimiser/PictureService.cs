using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;

namespace AssetOptimiser
{
  public class PictureService
  {
    private int webpQuality = 90;

    public PictureService(int quality) {
      webpQuality = quality;
    }

    record ConversionItem(string Directory, string FileName, string WithoutExtension, string RenderName = null);

    public List<PictureJob> GetPictures(string rootPath, bool isRender) =>
                Directory.EnumerateFiles(rootPath, "*.png", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(rootPath, "*.jpg", SearchOption.AllDirectories).Where(x => !x.Contains("postcard", StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => new ConversionItem(Path.GetDirectoryName(x), Path.GetFileName(x), Path.GetFileNameWithoutExtension(x)))
                    .Select(f => new PictureJob(f.FileName, f.Directory, isRender))
                    .ToList();

    public async Task ConvertPictures(List<PictureJob> pictures)
    {
      Console.WriteLine($"Converting pictures...");
      foreach (var picture in pictures.DistinctBy(x => x.RootFilename))
      {
        var normalExec = picture.GetWebpExecutionString(webpQuality, false);
        var halfsizeExec = picture.GetWebpExecutionString(webpQuality, true);
        var postcardExec = picture.GetJpegExecutionString(webpQuality);

        if (!File.Exists(picture.FullWebpPath))
        {
          await FFMpegProcessor.StartProcess(Paths.CwebpPath, picture.Directory, normalExec);
        }

        if (picture.IsRender)
        {
          if (!File.Exists(picture.HalfWebpPath))
          {
            await FFMpegProcessor.StartProcess(Paths.CwebpPath, picture.Directory, halfsizeExec);
          }

          if (!File.Exists(picture.PostcardPath))
          {
            await FFMpegProcessor.StartProcess(Paths.FFMpegPath, picture.Directory, postcardExec);
          }
        }
      }
    }
  }
}