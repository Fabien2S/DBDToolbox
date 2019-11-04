using System.Diagnostics;
using System.IO;
using NLog;

namespace DeadBySounds
{
    public class GameExtractor
    {
        private const string QuickBmsPath = @"bin\quickbms\quickbms.exe";
        private const string QuickBmsScriptPath = @"bin\quickbms\unreal_tournament_4.bms";


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _path;

        public GameExtractor(string path)
        {
            _path = path;
        }

        public bool Extract(string destination)
        {
            if (Directory.Exists(destination))
            {
                Logger.Debug("Cleaning directory \"{0}\" before extracting", destination);
                Directory.Delete(destination, true);
            }

            Logger.Debug("Directory \"{0}\" created", destination);
            Directory.CreateDirectory(destination);

            Logger.Info("Extracting game files from \"{0}\"", _path);
            Logger.Info("This may take a while, please be patient");

            if (!File.Exists(QuickBmsPath))
            {
                Logger.Error("Missing QuickBMS binary at \"{0}\"", QuickBmsPath);
                return false;
            }

            var process = Process.Start(new ProcessStartInfo(QuickBmsPath)
            {
                Arguments = '"' + QuickBmsScriptPath + "\" \"" + _path + "\" \"" + destination + '"',
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process == null)
            {
                Logger.Error("Unable to start QuickBMS");
                return false;
            }

            process.BeginOutputReadLine();
            process.OutputDataReceived += OnProcessMessage;

            process.WaitForExit();

            process.CancelOutputRead();
            process.OutputDataReceived -= OnProcessMessage;

            if (process.ExitCode != 0)
            {
                Logger.Error("Unable to extract the game (exit code: {0})", process.ExitCode);
                return false;
            }

            Logger.Info("Files successfully extracted in {0}", destination);
            return true;
        }

        private static void OnProcessMessage(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug("[QuickBMS] Extracting file: {0}", e.Data);
        }
    }
}