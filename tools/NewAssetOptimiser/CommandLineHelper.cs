using System;
using System.Linq;

namespace AssetOptimiser
{
    internal static class CommandLineHelper
    {
        public static bool ProcessCmdLineArgs(string[] args, out int? crf, out int webpQuality, out string rootPath)
        {
            crf = null;
            webpQuality = 90;
            if (args?.Any() != true)
            {
                Console.WriteLine("1: Root path to search, 2: (optional) CRF 0-51, 3: (optional) Q 1-100");
                rootPath = null;
                return false;
            }

            rootPath = args[0];
            if (args.Length > 1)
            {
                foreach (var arg in args.Skip(1))
                {
                    (var crfq, var webp) = ProcessArg(arg);
                    if (crfq.HasValue) crf = crfq.Value;
                    if (webp.HasValue) webpQuality = webp.Value;
                }
            }

            return true;
        }

        static (int? CRF, int? webpQuality) ProcessArg(string arg)
        {
            var isCRF = arg.ToLower().Contains("crf");
            if (isCRF)
            {
                if (!int.TryParse(arg.Replace("--crf=", ""), out int crf))
                    throw new ArgumentException("CRF is not formatted right. Should be '--crf=0-51'");

                return (crf, null);
            }

            var isQ = arg.ToLower().Contains("webpq");
            if (isQ)
            {
                if (!int.TryParse(arg.Replace("--webpq=", ""), out int webpQ))
                    throw new ArgumentException("webpQ is not formatted right. Should be '--webpq=1-100'");

                return (null, webpQ);
            }

            return (null, null);
        }
    }
}
