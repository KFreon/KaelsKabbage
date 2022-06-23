using System;
using System.IO;

namespace AssetOptimiser
{
    public enum Size {
        Normal,
        Halfsize = 1,
        Quartersize = 2
    }
    public class VideoJob : JobBase
    {
        
        public Format Format { get; }
        public string RootFilename => FilenameNoExt.Replace("_halfsize", "").Replace(Format.Postfix, "");
        
        public string PostcardPath => Path.Combine(Directory, RootFilename + "_postcard.jpg");

        public string FormattedPath(Size size)
        {
            var sizeString = size.ToString().ToLower();
            var sizeStringInterpolated = size == Size.Normal ? "" : $"_{sizeString}_";
            return Path.Combine(Directory, RootFilename + sizeStringInterpolated + $"{Format.Postfix}.{Format.Extension}");
        }

        public VideoJob(string filePath, Format format, bool isRender) 
            : base(Path.GetFileName(filePath), Path.GetDirectoryName(filePath), isRender)
        {
            Format = format;
        }

        public string GetCompressedVideoExecutionString(Size size) {
            var scale = size == Size.Normal ? "" : $"-vf scale=-1:{(size == Size.Halfsize ? "720" : "360")}:flags=lanczos";
            return Format.Name switch
            {
                // -vf scale=-1:250:flags=lanczos
                "AV1" => $"-i {FileName} -qp {Format.CRF} -c:v {Format.Encoder} {scale} -preset 3 {Path.GetFileName(FormattedPath(size))}",
                "VP9" => $"-i {FileName} -c:v {Format.Encoder} -crf {Format.CRF} {Format.Bitrate} {Format.AdditionalArguments} {Path.GetFileName(FormattedPath(size))}",
                _ => throw new ArgumentException("Unknown format")
            };
        }

        public string GetPostcardExecutionString() {
            return $"-i {FileName} -y -vframes 1 -vf scale=275:-1 {Path.GetFileName(PostcardPath)}";
        }
    }
}
