namespace AssetOptimiser
{
    static partial class Program
    {
        struct VideoJob
        {
            public string FileName { get; set; }
            public string Directory { get; set; }
            public Format Format { get; set; }
            public string DestinationFileName { get; set; }
        }
    }
}
