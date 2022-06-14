using System.IO;

namespace AssetOptimiser
{
    public struct VideoJob
    {
        public string FileName { get; set; }
        public string Directory { get; set; }
        public Format Format { get; set; }
        public string DestinationFileName { get; set; }
        public string FilenameNoExt => Path.GetFileNameWithoutExtension(FileName);
        public string PostcardDestination => $"{FilenameNoExt}_postcard.jpg";

        public string GetCompressedVideoExecutionString() {
            var bitrate = Format.Bitrate.HasValue ? $"-b:v {Format.Bitrate}" : "";
            var arg = $"-i {FileName} -c:v {Format.Encoder} -crf {Format.CRF} {bitrate} {Format.AdditionalArguments} {DestinationFileName}";
            return arg;
        }

        public string GetPostcardExecutionString() {
        return $"-i {FileName} -y -vframes 1 -vf scale=275:-1 {PostcardDestination}";
        }
    }
}
