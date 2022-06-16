using System.Diagnostics;
using System.IO;

namespace AssetOptimiser
{
    [DebuggerDisplay("{FileName}")]
    public abstract class JobBase 
    {
        public string FileName { get; }
        public string Directory { get; }
        public bool IsRender { get; }
        public string FilenameNoExt => Path.GetFileNameWithoutExtension(FileName);

        public JobBase(string fileName, string directory, bool isRender)
        {
            FileName = fileName;
            Directory = directory;
            IsRender = isRender;
        }
    }
}
