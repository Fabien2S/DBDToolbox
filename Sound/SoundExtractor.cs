using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using DeadBySounds.Debug;

namespace DeadBySounds.Sound
{
    public class SoundExtractor
    {
        private const string SoundBankFileName = "SoundbanksInfo.xml";
        private const string BnkExtrPath = @"bin\bnkextr.exe";

        private static readonly Logger Logger = Logger.GetLogger<SoundExtractor>();

        private readonly string _path;
        private readonly IReadOnlyCollection<SoundEntry> _streamedFiles;
        private readonly IReadOnlyCollection<SoundBank> _soundBanks;

        public SoundExtractor(string path, IReadOnlyCollection<SoundEntry> streamedFiles,
            IReadOnlyCollection<SoundBank> soundBanks)
        {
            _path = path;
            _streamedFiles = streamedFiles;
            _soundBanks = soundBanks;
        }

        public void ExtractStreamedFiles(string destination)
        {
            foreach (var streamedFile in _streamedFiles)
            {
                var sourceFileName = Path.Combine(_path, streamedFile.Id + ".wem");
                var destFileName = Path.Combine(destination, streamedFile.Path);

                try
                {
                    Logger.Debug("Moving the file {0} to {1}", sourceFileName, destFileName);

                    var destFileInfo = new FileInfo(destFileName);
                    destFileInfo.Directory?.Create();

                    FileHelper.MoveSafely(sourceFileName, destFileName);
                }
                catch (IOException e)
                {
                    Logger.Error("Unable to move the file \"{0}\": {1}", sourceFileName, e.Message);
                }
            }
        }

        public void ExtractSoundBanks(string temp, string destination)
        {
            foreach (var soundBank in _soundBanks)
            {
                var bnkFileName = Path.GetFileNameWithoutExtension(soundBank.Path);
                if (bnkFileName == null)
                {
                    Logger.Error("Bank name is null");
                    continue;
                }

                var outputDirectory = Path.Combine(temp, bnkFileName);

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.GetFullPath(BnkExtrPath),
                    Arguments = '"' + Path.Combine(_path, soundBank.Path) + '"',
                    WorkingDirectory = outputDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                if (process == null)
                {
                    Logger.Error("Unable to extract the sound bank");
                    continue;
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Logger.Error("Unable to extract the sound bank (exit code: {0})", process.ExitCode);
                    continue;
                }

                var i = 0;
                foreach (var entry in soundBank.Entries)
                {
                    i++;

                    var sourceFileName = Path.Combine(outputDirectory,
                        i.ToString("D4", CultureInfo.InvariantCulture) + ".wem");
                    var destFileName = Path.Combine(destination, entry.Path);

                    try
                    {
                        var destFileInfo = new FileInfo(destFileName);
                        destFileInfo.Directory?.Create();

                        FileHelper.MoveSafely(sourceFileName, destFileName);
                    }
                    catch (IOException e)
                    {
                        Logger.Error("Unable to move the file \"{0}\": {1}", sourceFileName, e.Message);
                    }
                }
            }
        }

        public void ExtractLeftOverSounds(string destination)
        {
            var files = Directory.GetFiles(_path, "*.wem", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var destFileName = Path.GetFileName(file) ?? FileHelper.RandomFileName() + ".wem";
                var destFile = Path.Combine(destination, destFileName);
                FileHelper.MoveSafely(file, destFile);
            }
        }
    }
}