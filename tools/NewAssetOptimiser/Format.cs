using System.Diagnostics;

namespace AssetOptimiser
{
    [DebuggerDisplay("{Name}")]
    public struct Format
    {
        public string Name { get; }
        public string Encoder { get; }
        public int CRF { get; }
        public int? Bitrate { get; }
        public string AdditionalArguments { get; }
        public string Extension { get; }
        public string Postfix => $"_{Name}";

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
}
