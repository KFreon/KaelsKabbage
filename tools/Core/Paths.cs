namespace Core
{
  public static class Paths
  {
    public static string BasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory());
    public static string ToolsPath => Path.Combine(BasePath, "Tools");
    public static string RendersFolder => Path.Combine(BasePath, "content/Renders");
    public static string RenderDump => Path.Combine(BasePath, ".RENDER_DUMP");
  }
}