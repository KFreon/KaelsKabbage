namespace Core
{
    public static class Paths
    {
        public static string BasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory());
        public static string ToolsPath => Path.Combine(BasePath, "Tools");
        public static string RendersFolder => Path.Combine(BasePath, "content/Renders");
        public static string PostsFolder => Path.Combine(BasePath, "content/posts");
        public static string RenderDump => Path.Combine(BasePath, ".RENDER_DUMP");

        private static string ffmpegPath = string.Empty;
        public static string FFMpegPath
        {
            get
            {
                if (Paths.ffmpegPath?.Length == 0)
                {
                    return Paths.ffmpegPath;
                }

                var path = Environment.GetEnvironmentVariable("PATH");
                var chocoBits = path.Split(';').Where(x => x.Contains("chocolatey\\bin", StringComparison.OrdinalIgnoreCase)).ToArray();

                var chocolateyBinPath = chocoBits.Single();
                var ffmpegPath = "";
                if (Directory.Exists(chocolateyBinPath))
                {
                    ffmpegPath = Path.Combine(chocolateyBinPath, "ffmpeg.exe");
                    if (!File.Exists(ffmpegPath))
                        throw new FileNotFoundException("ffmpeg is not installed at: " + ffmpegPath);
                }
                else
                    throw new DirectoryNotFoundException("Choco directory not found on PATH");

                Paths.ffmpegPath = ffmpegPath;
                return ffmpegPath;
            }
        }

        private static string cwebpPath = string.Empty;
        public static string CwebpPath
        {
            get
            {
                if (cwebpPath?.Length == 0)
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

        public static string FFProbePath => ffmpegPath.Replace("ffmpeg.exe", "ffprobe.exe", StringComparison.InvariantCultureIgnoreCase);
    }
}