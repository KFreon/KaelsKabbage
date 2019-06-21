using System.Diagnostics;

namespace AssetOptimiser
{
    static partial class Program
    {
        [DebuggerDisplay("{Name}")]
        struct Format
        {
            public string Name { get; }
            public string Encoder { get; }
            public int CRF { get; }
            public int? Bitrate { get; }
            public string AdditionalArguments { get; }
            public string Extension { get; }
            public string PostFix => Name=="x264" ? "" : $"_{Name}";
            public bool HasPostFix => !string.IsNullOrEmpty(PostFix);

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
}
