using System.IO;

namespace AssetOptimiser {
  public class PictureJob {
    public string Directory {get;}
    public string Filename {get;}
    public string FilenameNoExt => Path.GetFileNameWithoutExtension(Filename);
    public bool RequiresHalfsize => Directory.Contains("content\\Renders\\img");

    public PictureJob(string directory, string filename) {
      Directory = directory;
      Filename = filename;
    }

    public string GetExecutionString(int webpQuality, bool makeHalfsize) {
      var newPath = $"{FilenameNoExt}{(makeHalfsize ? "_halfsize" : "")}.webp";
      return $"{Filename} -o {newPath} {(makeHalfsize ? " -resize 900 0" : "")} -mt -m 6 -pass 10 -q {webpQuality}";
    }
  }
}