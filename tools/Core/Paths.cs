namespace Core
{
    public static class Paths
    {
        public static string BasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory());
        public static string ToolsPath => Path.Combine(BasePath, "tools");
        public static string RendersFolder => Path.Combine(BasePath, "content/renders");
        public static string PostsFolder => Path.Combine(BasePath, "content/posts");
        public static string RenderDump => Path.Combine(BasePath, ".RENDER_DUMP");

        private static string ffmpegPath = string.Empty;
        public static string FFMpegPath
        {
            get
            {
                if (Paths.ffmpegPath != null && Paths.ffmpegPath.Length != 0)
                {
                    return Paths.ffmpegPath;
                }

                var ffmpegPath = FindFFMpeg();
                Paths.ffmpegPath = ffmpegPath;
                return ffmpegPath;
            }
        }

        private static string cwebpPath = string.Empty;
        public static string CwebpPath
        {
            get
            {
                if (cwebpPath?.Length != 0)
                {
                    return cwebpPath;
                }

                var cwebp = Path.Combine(Core.Paths.ToolsPath, "Webp", "cwebp.exe");
                if (!File.Exists(cwebp))
                    throw new FileNotFoundException("Webp converter not found at: " + cwebp);

                cwebpPath = cwebp;
                return cwebp;
            }
        }

        public static string FFProbePath => FFMpegPath.Replace("ffmpeg.exe", "ffprobe.exe", StringComparison.InvariantCultureIgnoreCase);
        
        private static string? FindFFMpeg() {
            if (LookForFFMPegOnChoco(out var chocoFFMpeg)) {
                return chocoFFMpeg;
            }

            if (LookForFFMPegOnWinget(out var wingetFFMpeg)) {
                return wingetFFMpeg;
            }

            if (LookForFFMPegOnLinux(out var linuxFFMpeg)) {
                return linuxFFMpeg;
            }

            return null;
        }

        private static bool LookForFFMPegOnLinux(out string ffmpegPath) {
            ffmpegPath = string.Empty;
            var testPath = "/usr/bin/ffmpeg";
            if (File.Exists(testPath)) {
                ffmpegPath = testPath;
                return true;
            }
            return false;
        }

        private static bool LookForFFMPegOnWinget(out string ffmpegPath) {
            ffmpegPath = string.Empty;
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            
            if (localAppData == null) return false;

            var defaultWingetDirectory = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
            if (!string.IsNullOrEmpty(defaultWingetDirectory) && Directory.Exists(defaultWingetDirectory)) {
                var candidates = Directory.GetFiles(defaultWingetDirectory, "ffmpeg.exe", SearchOption.AllDirectories);
                var first = candidates.FirstOrDefault();
                if (!string.IsNullOrEmpty(first)) {
                    ffmpegPath = first;
                    return true;
                }
            }

            return false;
        }

        private static bool LookForFFMPegOnChoco(out string ffmpegPath) {
            ffmpegPath = string.Empty;
            var path = Environment.GetEnvironmentVariable("PATH");
            var chocoBits = path!.Split(';').Where(x => x.Contains("chocolatey\\bin", StringComparison.OrdinalIgnoreCase)).ToArray();
            var chocolateyBinPath = chocoBits.SingleOrDefault();
            if (string.IsNullOrEmpty(chocolateyBinPath) || !Directory.Exists(chocolateyBinPath)) {
                return false;
            }

            var temp = Path.Combine(chocolateyBinPath, "ffmpeg.exe");
            if (File.Exists(temp))
                ffmpegPath = temp;

            var isFound = !string.IsNullOrEmpty(ffmpegPath);
            return isFound;
        }
    }
}