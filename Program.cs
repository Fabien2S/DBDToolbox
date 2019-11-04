using System;
using System.IO;
using DeadBySounds.Debug;
using DeadBySounds.Sound;

namespace DeadBySounds
{
    internal class Program
    {
        private const string UnsupportedVersionMessage =
            "This version of Dead by Daylight may be unsupportd for now";

        private const string DefaultGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Dead by Daylight";
        private const string AudioPakPath = @"DeadByDaylight\Content\Paks\pakchunk2-WindowsNoEditor.pak";

        private const string ExtractedGameDirectory = "extracted_game";
        private const string SoundDirectory = @"Content\WwiseAudio\Windows";

        private const string TempDirectory = "temp";
        private const string OutputDirectory = "output";

        private static readonly Logger Logger = Logger.GetLogger<Program>();

        public static void Main(string[] args)
        {
            var gamePath = args.Length >= 1 ? Path.GetFullPath(args[0]) : DefaultGamePath;
            if (!Directory.Exists(gamePath))
            {
                Logger.Error("Invalid game path \"{0}\" (Missing directory)", gamePath);
                Environment.Exit(1);
                return;
            }

            var extractedGamePath = Path.GetFullPath(ExtractedGameDirectory);
            if (!Directory.Exists(extractedGamePath))
            {
                Logger.Debug("Using game path: \"{0}\"", gamePath);

                var pakPath = Path.Combine(gamePath, AudioPakPath);
                if (!File.Exists(pakPath))
                {
                    Logger.Error("Missing audio pak file in \"{0}\" ({1})", pakPath, UnsupportedVersionMessage);
                    Environment.Exit(1);
                    return;
                }

                Logger.Debug("Sounds PAK file found: \"{0}\"", pakPath);

                // EXTRACTING GAME FILES       

                var extractor = new GameExtractor(pakPath);
                if (!extractor.Extract(extractedGamePath))
                {
                    Logger.Error("Unable to extract the game files");
                    Environment.Exit(1);
                    return;
                }
            }
            else
                Logger.Debug("Using extracted game path: \"{0}\"", extractedGamePath);

            // PARSING SOUND BANKS FILE
            var soundsPath = Path.Combine(extractedGamePath, SoundDirectory);
            if (!Directory.Exists(soundsPath))
            {
                Logger.Error("Missing sounds folder at \"{0}\" ({1})", soundsPath, UnsupportedVersionMessage);
                Environment.Exit(1);
                return;
            }

            var soundParser = new SoundParser(soundsPath);

            var streamedSounds = soundParser.ParseStreamedSounds();
            if (streamedSounds == null)
            {
                Logger.Error("Unable to parse the streamed sounds");
                Environment.Exit(1);
                return;
            }

            var soundBanks = soundParser.ParseBanks();
            if (soundBanks == null)
            {
                Logger.Error("Unable to parse the sound banks");
                Environment.Exit(1);
                return;
            }

            // EXTRACTING SOUND FILES
            var tmpDirectory = Path.GetFullPath(TempDirectory);
            var outputDirectory = Path.GetFullPath(OutputDirectory);

            Directory.CreateDirectory(tmpDirectory);
            Directory.CreateDirectory(outputDirectory);

            var soundExtractor = new SoundExtractor(soundsPath, streamedSounds, soundBanks);
            soundExtractor.ExtractStreamedFiles(outputDirectory);
            soundExtractor.ExtractSoundBanks(tmpDirectory, outputDirectory);
            soundExtractor.ExtractLeftOverSounds(outputDirectory);

            var soundProcessor = new SoundProcessor(outputDirectory);
            soundProcessor.ProcessSounds();

            Logger.Success("Done.");

            Console.ReadKey();
        }
    }
}