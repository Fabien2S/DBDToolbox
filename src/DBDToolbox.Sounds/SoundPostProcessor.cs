using System.Diagnostics;
using System.IO;
using System.Xml;
using DBDToolbox.Assets;
using DBDToolbox.Logging;
using DBDToolbox.Postprocessors;
using DBDToolbox.Sounds.Assets;
using Microsoft.Extensions.Logging;

namespace DBDToolbox.Sounds
{
    public class SoundPostProcessor : IPostProcessor
    {
        private static readonly ILogger<SoundPostProcessor> Logger = LogManager.Create<SoundPostProcessor>();

        private const string SoundBankExtension = ".bnk";
        private const string SoundInfoFileExtension = ".xml";

        private const string SoundFileSearchPattern = "*.wem";

        public bool CanProcess(AssetPath path)
        {
            return path.EndsWith(SoundBankExtension);
        }

        public void Process(AssetPath path, IAsset asset)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var directoryName = Path.GetDirectoryName(path);

            var infoFilePath = Path.Join(directoryName, fileName + SoundInfoFileExtension);
            var infoDocument = new XmlDocument();

            Logger.LogDebug("Loading bank info from \"{0}\"", infoDocument);
            infoDocument.Load(infoFilePath);

            var soundBankAsset = (SoundBankAsset)asset;
            var soundBankElement = infoDocument.SelectSingleNode("//SoundBank[@Id=" + soundBankAsset.BankId + "]");
            if (soundBankElement == null)
            {
                Logger.LogError("Missing bank info for {0}", path);
                return;
            }

            var files = Directory.EnumerateFiles(path, SoundFileSearchPattern, SearchOption.AllDirectories);
            foreach (var wemFile in files)
            {
                var tempPath = Path.ChangeExtension(wemFile, "temp");
                var oggPath = Path.ChangeExtension(wemFile, "ogg");
            
                if(!RunProcess("ww2ogg.exe", $"\"{wemFile}\" --pcb \"packed_codebooks_aoTuV_603.bin\" -o \"{tempPath}\""))
                    continue;
                File.Delete(wemFile);
            
                if(!RunProcess("ReVorb.exe", $"\"{tempPath}\" \"{oggPath}\""))
                    continue;
                File.Delete(tempPath);
            
                var fileId = Path.GetFileNameWithoutExtension(oggPath);
                var fileInfoElement = soundBankElement.SelectSingleNode("//File[@Id=" + fileId + "]");
                if (fileInfoElement == null)
                {
                    Logger.LogError("Missing file info for {0}", path);
                    continue;
                }
                
                var fileNameElement = fileInfoElement["ShortName"];
                if(fileNameElement?.InnerText == null)
                {
                    Logger.LogError("Missing file path info for {0}", path);
                    continue;
                }

                var remapPath = Path.Join(path, fileNameElement.InnerText);
                var remapDirectory = Path.GetDirectoryName(remapPath);
                if (remapDirectory != null)
                    Directory.CreateDirectory(remapDirectory);
                File.Move(oggPath, remapPath);
            }
        }

        private static bool RunProcess(string fileName, string arguments)
        {
            var process = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process == null)
            {
                Logger.LogError("Failed to run {0}", fileName);
                return false;
            }

            process.BeginOutputReadLine();
            process.OutputDataReceived += HandleProcessOutput;
            process.WaitForExit();
            process.CancelOutputRead();
            process.OutputDataReceived -= HandleProcessOutput;

            if (process.ExitCode == 0)
                return true;

            Logger.LogError("Failed to run {0} (exit code: {1})", fileName, process.ExitCode);
            return false;
        }

        private static void HandleProcessOutput(object sender, DataReceivedEventArgs e)
        {
            var name = ((Process)sender).StartInfo.FileName;
            Logger.LogDebug("[{0}] {1}", name, e.Data);
        }
    }
}