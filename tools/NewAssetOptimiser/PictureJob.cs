using System.IO;

namespace AssetOptimiser
{
    public class PictureJob : JobBase
    {
        public string RootFilename => FilenameNoExt.Replace("_halfsize", "");
        public string FullWebpPath => Path.Combine(Directory, RootFilename + ".webp");
        public string HalfWebpPath => Path.Combine(Directory, RootFilename + "_halfsize.webp");
        public string PostcardPath => Path.Combine(Directory, RootFilename + "_postcard.jpg");

        public PictureJob(string filename, string directory, bool isRender) : base(filename, directory, isRender)
        {
        }

        public string GetWebpExecutionString(int webpQuality, bool makeHalfsize)
        {
            var newPath = makeHalfsize ? HalfWebpPath : FullWebpPath;
            return $"{FileName} -o {Path.GetFileName(newPath)} {(makeHalfsize ? " -resize 0 250" : "")} -mt -m 6 -af -pass 10 -q {webpQuality}";
        }

        public string GetJpegExecutionString(int quality)
        {
            return $"-i {FileName} -y -vf scale=275:-1 {Path.GetFileName(PostcardPath)}";
        }
    }
}