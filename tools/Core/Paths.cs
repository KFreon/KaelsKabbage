namespace Core
{
  public static class Paths
  {
    public static string BasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory());
    public static string ToolsPath => Path.Combine(BasePath, "Tools");
    public static string RendersFolder => Path.Combine(BasePath, "content/Renders");
    public static string RenderDump => Path.Combine(BasePath, ".RENDER_DUMP");

        public static string GetFFMpegPath()
        {
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

            return ffmpegPath;
        }
    }
}