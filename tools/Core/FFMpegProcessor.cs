using System.Diagnostics;
using System.Text;

namespace Core
{
    public static class FFMpegProcessor
    {
        public static Task StartProcess(string tool, string workingDirectory, string argument, StringBuilder? stdOut = null)
        {
            void writer(string? str)
            {
                if (stdOut == null)
                    Console.WriteLine(str);
                else
                    stdOut.AppendLine(str);
            }


            writer("");
            writer($"Running: {Environment.NewLine}" +
                $"Tool: {Path.GetFileNameWithoutExtension(tool)} {Environment.NewLine}" +
                $"WorkingDir: {workingDirectory} {Environment.NewLine}" +
                $"Args: {argument}");

            var psi = new ProcessStartInfo(tool);
            psi.WorkingDirectory = workingDirectory;
            psi.CreateNoWindow = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            psi.Arguments = argument;
            var process = new Process()
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (sender, data) => writer(data.Data);

            if (stdOut == null)
                process.ErrorDataReceived += (o, e) => FFMpegOutputWriter(e, writer);

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        private static void FFMpegOutputWriter(DataReceivedEventArgs e, Action<string?> writer)
        {
            if (e?.Data == null)
                return;

            if (e.Data.StartsWith("frame="))
                Console.SetCursorPosition(0, Console.CursorTop - 1);

            writer(e.Data);
            Console.Out.Flush();
        }
    }
}
