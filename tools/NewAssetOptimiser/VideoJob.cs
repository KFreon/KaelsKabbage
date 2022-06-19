using System;
using System.IO;

namespace AssetOptimiser
{
    public class VideoJob : JobBase
    {
        
        public Format Format { get; }
        public string RootFilename => FilenameNoExt.Replace("_halfsize", "").Replace(Format.Postfix, "");
        public string FormattedPath => Path.Combine(Directory, RootFilename + $"{Format.Postfix}.{Format.Extension}");
        public string PostcardPath => Path.Combine(Directory, RootFilename + "_postcard.jpg");

        public VideoJob(string filePath, Format format, bool isRender) 
            : base(Path.GetFileName(filePath), Path.GetDirectoryName(filePath), isRender)
        {
            Format = format;
        }

        public string GetCompressedVideoExecutionString() {
            return Format.Name switch
            {SCALE
                "AV1" => $"-i {FileName} -qp 45 -c:v {Format.Encoder} -SCALE -preset 3 {Path.GetFileName(FormattedPath)}",
                "VP9" => $"-i {FileName} -c:v {Format.Encoder} -crf {Format.CRF} {Format.Bitrate} {Format.AdditionalArguments} {Path.GetFileName(FormattedPath)}",
                _ => throw new ArgumentException("Unknown format")
            };
        }

        public string GetPostcardExecutionString() {
            return $"-i {FileName} -y -vframes 1 -vf scale=275:-1 {Path.GetFileName(PostcardPath)}";
        }
    }
}
