using System.IO;

namespace AssetOptimiser {
  public class PictureJob {
    public string Directory {get;}
    public string Filename {get;}
    public string FilenameNoExt => Path.GetFileNameWithoutExtension(Filename);

    public PictureJob(string directory, string filename) {
      Directory = directory;
      Filename = filename;
    }

    public string GetWebpExecutionString(int webpQuality, bool makeHalfsize) {
      var newPath = $"{FilenameNoExt}{(makeHalfsize ? "_halfsize" : "")}.webp";
      return $"{Filename} -o {newPath} {(makeHalfsize ? " -resize 450 0" : "")} -mt -m 6 -pass 10 -q {webpQuality}";
    }

    public string GetJpegExecutionString(int quality) {
      var newPath = $"{FilenameNoExt}_postcard.jpg";
      return $"-i {Filename} -y -vf scale=275:-1 {newPath}";
    }
  }
}