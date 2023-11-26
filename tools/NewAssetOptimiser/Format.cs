using System.Diagnostics;

namespace AssetOptimiser
{
    [DebuggerDisplay("{Name}")]
    public record struct Format(string Name, string Extension, string Encoder, int CRF, int? Bitrate, string AdditionalArguments)
    {
        public string Postfix => $"_{Name}";
    }
}
