using System.Diagnostics;
using System.IO;
using NLog;

namespace DeadBySounds.Sound
{
    public class SoundProcessor
    {
        private const string Ww2OggPath = @"bin\ww2ogg\ww2ogg.exe";
        private const string Ww2OggBinary = @"bin\ww2ogg\packed_codebooks_aoTuV_603.bin";

        private const string RevorbPath = @"bin\revorb.exe";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _path;

        public SoundProcessor(string path)
        {
            _path = path;
        }

        public void ProcessSounds()
        {
            var files = Directory.GetFiles(_path, "*.wem", SearchOption.AllDirectories);
            foreach (var wemFile in files)
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = Ww2OggPath,
                    Arguments = '"' + wemFile + "\" --pcb \"" + Path.GetFullPath(Ww2OggBinary),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                if (process == null)
                {
                    Logger.Error("Unable to convert the sound");
                    continue;
                }

                process.BeginOutputReadLine();
                process.OutputDataReceived += OnWw2OggMessage;

                process.WaitForExit();

                process.CancelOutputRead();
                process.OutputDataReceived -= OnWw2OggMessage;

                if (process.ExitCode != 0)
                {
                    Logger.Error("Unable to convert the sound (exit code: {0})", process.ExitCode);
                    continue;
                }

                var oggFile = Path.ChangeExtension(wemFile, "ogg");
                if (oggFile == null)
                {
                    Logger.Error("Unable to change the file extension");
                    continue;
                }

                process = Process.Start(new ProcessStartInfo
                {
                    FileName = RevorbPath,
                    Arguments = oggFile,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                if (process == null)
                {
                    Logger.Error("Unable to convert the sound");
                    continue;
                }

                process.BeginOutputReadLine();
                process.OutputDataReceived += OnRevorbMessage;

                process.WaitForExit();

                process.CancelOutputRead();
                process.OutputDataReceived -= OnRevorbMessage;

                if (process.ExitCode != 0)
                    Logger.Error("Unable to convert the sound (exit code: {0})", process.ExitCode);

                FileHelper.DeleteSafely(wemFile);
            }
        }

        private static void OnWw2OggMessage(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug("[WW2OGG] Converting file (1/2): {0}", e.Data);
        }

        private static void OnRevorbMessage(object sender, DataReceivedEventArgs e)
        {
            Logger.Debug("[Revorb] Converting file (2/2): {0}", e.Data);
        }
    }
}